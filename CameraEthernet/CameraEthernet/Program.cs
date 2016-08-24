using System;
using System.Net;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using System.Net.Sockets;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT.Net.NetworkInformation;

namespace CameraEthernet
{
    public partial class Program
    {

        Gadgeteer.Networking.WebEvent test;
        Gadgeteer.Networking.WebEvent seePicture;

        GT.Picture pic = null;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            
            ethernetJ11D.NetworkInterface.Open();
            ethernetJ11D.UseThisNetworkInterface();

            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            // loop through each interface, assign an IP accordingly
            foreach (NetworkInterface ni in networkInterfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    ni.EnableStaticIP("169.254.120.154" , "255.255.0.0" , "192.168.1.1");
                    ni.EnableStaticDns(new string[] { "8.8.8.8" , "8.8.4.4" });
                }
            }

 
            ethernetJ11D.NetworkUp += new GTM.Module.NetworkModule.NetworkEventHandler(ethernet_NetworkUp);
            ethernetJ11D.NetworkDown += new GTM.Module.NetworkModule.NetworkEventHandler(ethernet_NetworkDown);

            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

        
        }

 
        void camera_PictureCaptured(Camera sender, GT.Picture picture)
        {
            pic = picture;
            displayTE35.SimpleGraphics.DisplayImage(picture, 5, 5);
        }


        void ethernet_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Network is Up with following properties");
            Debug.Print("IP:" + ethernetJ11D.NetworkSettings.IPAddress.ToString());

            Gadgeteer.Networking.WebServer.StartLocalServer(ethernetJ11D.NetworkSettings.IPAddress , 80);

            foreach (string s in sender.NetworkSettings.DnsAddresses)
            Debug.Print("Dns:" + s);

            var NetworkSettings = ethernetJ11D.NetworkSettings;
            Debug.Print("IP Address: " + NetworkSettings.IPAddress);
            Debug.Print("Subnet Mask: " + NetworkSettings.SubnetMask);
            Debug.Print("Gateway: " + NetworkSettings.GatewayAddress);
         
            test = Gadgeteer.Networking.WebServer.SetupWebEvent("test");
            test.WebEventReceived += new WebEvent.ReceivedWebEventHandler(test_WebEventReceived);

            seePicture = Gadgeteer.Networking.WebServer.SetupWebEvent("seepicture");
            seePicture.WebEventReceived += new WebEvent.ReceivedWebEventHandler(seePicture_WebEventReceived);

            button.TurnLedOn();
            
        }

        void ethernet_NetworkDown(GTM.Module.NetworkModule sender , GTM.Module.NetworkModule.NetworkState state)
        {
            button.TurnLedOff();
            Debug.Print("Network Down");
        }

        
        void seePicture_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
           if (pic != null)
                responder.Respond(pic);

           else
                responder.Respond("Take picture first");
        }

        void test_WebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            Debug.Print("Hello world");
            string content = "<html><body><h1>Hosted on .NET Gadgteer!!</h1><a href=\"http://169.254.120.154/seepicture\">http://169.254.120.154/seepicture</a></body></html>";
            byte[] bytes = new System.Text.UTF8Encoding().GetBytes(content);
            responder.Respond(bytes , "text/html");
            
            
        }

        void button_ButtonPressed(Button sender , Button.ButtonState state)
        {            
            camera.TakePicture();
        }
    }
}
