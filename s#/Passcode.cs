using System;
using System.Text;
using Crestron.SimplSharp;                   
using Crestron.SimplSharpPro;                
using Crestron.SimplSharpPro.Diagnostics;	
using Crestron.SimplSharpPro.DeviceSupport;  


public class Passcode
{
    /// this is the hardcoded backdoor pin, change it :)
    private const string codeBackdoor = "0000";             

    private BasicTriList TP;
    private SmartObject Keypad;
    private volatile uint textSerialJoin;
    private volatile uint changeCodeJoin;     
  
    public volatile bool setNewCode;
    public volatile bool newCodeEntry;    
    private volatile string codeEntered;
    private volatile string code = codeBackdoor; 
    private volatile uint codeEnteredLength;
    private const uint codeMaxLength = 5;

    public bool debug_enable { get; set; }

    Action CorrectCodeEntered;


    /// <summary>
    /// Initialises by receiving touch panel definition, keypad smart object, 
    /// change code digital join and text field serial join. 
    /// </summary>
    public void Init(BasicTriList device, SmartObject keypad, uint changeCode, uint serialjoin)
    {
        TP = device;
        Keypad = keypad;
        changeCodeJoin = changeCode;
        textSerialJoin = serialjoin;        
     
        Keypad.SigChange += PasscodeKeypadSigChange;
        TP.SigChange += new SigEventHandler(TPSigChangeHandler);        
    }
    

    public void SetCallback(Action CorrectCodeEntered)
    {
        this.CorrectCodeEntered = CorrectCodeEntered;
    }


    /// <summary>
    /// Outputs debugging information to the Console (if enabled)
    /// </summary>
    /// <param name="message">Message to print to console.</param>
    private void Debug(string message)
    {
        if (debug_enable)
        {
            CrestronConsole.PrintLine("[PASSCODE] " + message);
        }
    }

   
    /// <summary>
    /// Method to process signal changes from keypad smartobject
    /// </summary>
     public void PasscodeKeypadSigChange(GenericBase currentDevice, SmartObjectEventArgs args)
    {
        try
        {
            if (args.Sig.BoolValue) 
            {
                if (args.Sig.Name == "Misc_1") // clear button
                {
                    ClearEntry();
                    return;
                }

                if (args.Sig.Name == "Misc_2") // ok button
                {
                    if (newCodeEntry)
                    {
                        code=codeEntered;
                        Debug("Code has been changed");
                        PopulateTextField("New code saved");
                        newCodeEntry = false;
                        CrestronEnvironment.Sleep(2000);
                        ClearEntry();
                        return;
                    }

                    if (setNewCode)
                    {
                        if (codeEntered == code)
                        {
                            ClearEntry();
                            PopulateTextField("Enter new code");
                            setNewCode = false;
                            newCodeEntry = true;
                            return;
                        }
                        else
                        {
                            PopulateTextField("Incorrect code!");
                            CrestronEnvironment.Sleep(1000);
                            ClearEntry();         
                        }
                        TP.BooleanInput[changeCodeJoin].BoolValue = false;
                    }

                    if (!setNewCode && !newCodeEntry)
                    {
                        if (codeEntered == code || codeEntered == codeBackdoor)
                        {
                            ClearEntry();
                            Debug("Correct code entered!");
                            CorrectCodeEntered();
                        }
                        else
                        {
                            PopulateTextField("Incorrect code!");
                            Debug("Incorrect code entered!"); 
                            CrestronEnvironment.Sleep(1000);
                            ClearEntry();
                        }
                    }

                    return;
                }


                if (codeEnteredLength < codeMaxLength)
                {
                    codeEntered = string.Concat(codeEntered, args.Sig.Name);
                    Debug("new code entered = " + codeEntered);
                    codeEnteredLength++;

                    if (newCodeEntry)
                    {
                        PopulateTextField(codeEntered);
                    }
                    else
                    {
                        PopulateTextField(CodeToStars(codeEntered));
                    }
                }
                else
                {
                    PopulateTextField("Error: maximum 5 digits");
                    CrestronEnvironment.Sleep(2000);
                    ClearEntry();
                }
            }
        }
        catch (Exception ex)
        {
            Debug("PasscodeKeypadSigChange()exception: " + ex);
        }
    }


    /// <summary>
    /// Method to handle events from the 'change code' button press
    /// </summary>
    private void TPSigChangeHandler(BasicTriList TP, SigEventArgs args)
    {
        if ((args.Sig.Type == eSigType.Bool) && (args.Sig.BoolValue))
        {
            if (args.Sig.Number == changeCodeJoin)
            {
                setNewCode = !setNewCode;

                if (setNewCode)
                {
                    ClearEntry();
                    TP.BooleanInput[args.Sig.Number].BoolValue = true;
                    PopulateTextField("Enter current code");
                }
                else
                {
                    TP.BooleanInput[args.Sig.Number].BoolValue = false;
                    ClearEntry();
                }
            }
        }
    }
       
    /// <summary>
    /// Clears the passcode text field
    /// </summary>
    private void ClearEntry()
    {
        codeEntered = "";
        codeEnteredLength = 0;
        PopulateTextField(codeEntered);
        TP.BooleanInput[changeCodeJoin].BoolValue = false;
    }

 
    /// <summary>
    /// Displays text in the passcode text field 'textSerialJoin' for user feedback
    /// </summary>
    private void PopulateTextField(string Str)
    {                
        TP.StringInput[textSerialJoin].StringValue = Str;
    }


    /// <summary>
    /// Displays current entered digits as stars in the passcode text field
    /// </summary>
    private string CodeToStars(string code)
    {
        string stars;

        stars = "";

        for (int ctr = 1; ctr <= code.Length; ctr++)
        {
            stars = string.Concat(stars, "*");
        }
        return stars;
    }

}
        

