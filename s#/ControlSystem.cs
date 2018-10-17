#region References
using System;
using System.Text;
using Crestron.SimplSharp;                          	
using Crestron.SimplSharp.CrestronIO;                   
using Crestron.SimplSharp.CrestronSockets;              
using Crestron.SimplSharpPro;                       	
using Crestron.SimplSharpPro.CrestronThread;        	
using Crestron.SimplSharpPro.Diagnostics;		    	
using Crestron.SimplSharpPro.DeviceSupport;         	
using Crestron.SimplSharpPro.UI;                    	
#endregion


namespace PasscodeExample
{
    public class ControlSystem : CrestronControlSystem
    {
        Tsw760 tsw760;
        Passcode PIN;              
        SmartObject PasscodeKeypad;
                
        private const uint passcodeTextField = 1234;
        private const uint passcodeChangeButton = 1234;

        private const uint keypadID = 1; // smart graphic object id

                
        public ControlSystem()  : base()
        {
            Thread.MaxNumberOfUserThreads = 60;

            if (this.SupportsEthernet) 
            {
                tsw760 = new Tsw760(0x05, this);                                                   
                string TSW760SgdFilePath = string.Format("{0}\\Passcode.sgd", Directory.GetApplicationDirectory());

                if (File.Exists(TSW760SgdFilePath))
                {
                    tsw760.LoadSmartObjects(TSW760SgdFilePath);
                    CrestronConsole.PrintLine("[SYSTEM] TSW SGD FOUND!!!");
                    PasscodeKeypad = tsw760.SmartObjects[keypadID];
                }
                else
                    CrestronConsole.PrintLine("[SYSTEM] Could not find TSW SGD file. TSW SmartObjects will not work at this time");

                                
                if (tsw760.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
                    CrestronConsole.PrintLine("[SYSTEM] tsw760 has been registered successfully");
                else
                    CrestronConsole.PrintLine("[SYSTEM] tsw760 failed registration. Cause: {0}", tsw760.RegistrationFailureReason);
            }
            else
            {
                CrestronConsole.PrintLine("[SYSTEM] This processor does not support ethernet, so this program will not run");
            }


            /// set up passcode
            PIN = new Passcode();
            PIN.Init(tsw760, PasscodeKeypad, passcodeChangeButton, passcodeTextField);
            PIN.debug_enable = true;
            PIN.SetCallback(PINCodeEntered);
        }

 

        private void PINCodeEntered()
        {
            CrestronConsole.PrintLine("Correct PIN entered!");

            /// do something. 
        }


    }
}
