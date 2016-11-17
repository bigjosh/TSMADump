using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

namespace TSMADump
{
    class Program
    {

        static void p(String s)
        {
            Console.WriteLine(s);

        }

        abstract class Command : IComparable<Command>
        {


            // New commands must implement these...

            public abstract string getCommand() ;

            public abstract string getHelp();

            public abstract String[] getArgs();

            public abstract int function(String[] args);

            // ....

            public static String LF = "\r\n";

            System.IO.StreamWriter streamWriter = null;

            bool commaFlag;
            bool quoteFlag;

            protected String username;
            protected String password;
            protected String baseURL;

            public void setup(String baseURL, String username, String password, bool quoteFlag, bool commaFlag, String outFile)
            {

                this.baseURL = baseURL;
                this.username = username;
                this.password = password;
                this.commaFlag = commaFlag;
                this.quoteFlag = quoteFlag;

                if (outFile != null)
                {

                    streamWriter = new System.IO.StreamWriter(outFile);

                }
                else
                {
                    streamWriter = null;

                }

            }

            public void printHelp()
            {
                p("Usage for command: " + getCommand());
                p(getHelp());
                p("");
                p("Arguments:");

                foreach(String s in getArgs())
                {

                    p(s);

                }

            }
        
            protected void output(String s)
            {

                if (streamWriter != null)
                {

                    streamWriter.WriteLine(s);

                }
                else
                {

                    System.Console.WriteLine(s);

                }

            }
            
            public const char QUOTE = '\"';

            public String quotedList( params String[] sl  ) {

                StringBuilder sb = new StringBuilder();

                String buffer = "";

                foreach (String s in sl)
                {

                    if (quoteFlag) {

                        sb.Append( buffer + QUOTE + s + QUOTE);

                    } else {

                        sb.Append( buffer + s );

                    }

                    if (commaFlag) {

                        buffer = " , ";

                    } else {

                        buffer = " ";
                    }

                }

                return (sb.ToString());

            }


            // Returns true if correct number of args

            Boolean checkArgs( String[] args ) {

                if (args.Length != getArgs().Length ) {

                    p("Incorrect number of args specified for "+getCommand()+" command.");
                    p("Try no args for help on this command.\r\n");
                    return(false);

                }

                return(true);

            }

            public int execute( String[] args)
            {

                if (!checkArgs( args ) ) {

                    return(0);

                }

                int x=0;

                try
                {

                    x = function(args);

                }
                catch (Exception ex)
                {

                    p("Error:" + ex.ToString());

                }

                if (streamWriter != null)
                {
                    streamWriter.Close();
                }

                return( x );

            }

            public int CompareTo(Command x)
            {
                return getCommand().CompareTo(x.getCommand());
            }
        }


        class UserAdminCommand : Command {

            public override String getCommand()
            {
                return "USER";
            }

            public override String getHelp() {

                return (
                    "Dump all users in a domain." +LF+
                    "Username/password provided must be either domain or system admin." +LF+
                    LF+
                    "Outputs line for each user in the domain, each line has the fields..." +LF+
                    "UserName Password IsDomainAdmin IsSystemAdmin FirstName LastName"
                  );
            }

            public override String[] getArgs()
            {
                return(
                    new String[] {

                        "Domain - the name of the domain or domain alias to list"

                    }
                );

            }
            

            public override int function( String[] args )
            {

                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Fetchcing users from URL [" + a.Url +"]" );

                SvcUserAdmin.UserInfoListResult userInfoListResult = a.GetUsers(  username , password , args[0] );

                if (!userInfoListResult.Result)
                {
                    throw new Exception( userInfoListResult.Message);
                }

                if (userInfoListResult.Users != null)
                {

                    p("Processing " + userInfoListResult.Users.Length + " result records...");

                    foreach (SvcUserAdmin.UserInfo ui in (userInfoListResult.Users))
                    {

                        output(quotedList(ui.UserName, ui.Password, ui.IsDomainAdmin.ToString(), ui.IsSystemAdmin.ToString(), ui.FirstName, ui.LastName));
                    }
                } else {

                    p("No users found.");

                }

                p("Completed successfully");

                return 1;

            }

        }


        class UserForwardCommand : Command
        {

            public override String getCommand()
            {
                return "USERFORWARD";
            }

            public override String getHelp()
            {

                return (
                    "Get forwarding info for a specific user." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    LF +
                    "Outputs line containing the fields..." + LF +
                    "Address ForwardingEnabled DeleteOnForward ForwardingAddress " +LF+ 
                    LF+
                    "Nothing will be output if the specified user is not found."
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "UserName - the email address to get forwarding info for"

                    }
                );

            }


            public override int function(String[] args)
            {


                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Fetchcing user from URL [" + a.Url + "]");

                SvcUserAdmin.UserForwardingInfoResult userForwardingInfoResult= a.GetUserForwardingInfo(username, password, args[0]);

                if (!userForwardingInfoResult.Result)
                {

                    throw new Exception(userForwardingInfoResult.Message);

                }

                if (userForwardingInfoResult.ForwardingAddress != null)
                {

                    p("Processing result...");

                    bool forwardingEnabled = userForwardingInfoResult.ForwardingAddress.Length > 0;

                    output(quotedList(args[0], forwardingEnabled.ToString(), userForwardingInfoResult.DeleteOnForward.ToString(), userForwardingInfoResult.ForwardingAddress));

                }
                else
                {

                    p("User not found.");

                }
                p("Completed successfully");

                return 1;

            }

        }


        class UpdateUserForwardCommand : Command
        {

            public override String getCommand()
            {
                return "UPDATEUSERFORWARD";
            }

            public override String getHelp()
            {

                return (
                    "Update forwarding info for a specific user." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    "Specify a forwarding address to enable forwarding." +LF+
                    LF +
                    "No output if successful."
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "UserName  - the email address to update forwarding info for",
                        "Delete    - Delete the email after forwarding?",
                        "Forward   - destination email address to forward to",

                    }
                );

            }


            public override int function(String[] args)
            {


                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Fetchcing user from URL [" + a.Url + "]");

                SvcUserAdmin.GenericResult genericResult = a.UpdateUserForwardingInfo(username, password, args[0], args[1].Equals("Y") || args[1].Equals("y"), args[2] );

                if (!genericResult.Result)
                {

                    throw new Exception(genericResult.Message);

                }

                p("Completed successfully");

                return 1;

            }

        }

        class UpdateUserForward2Command : Command
        {

            public override String getCommand()
            {
                return "UPDATEUSERFORWARD2";
            }

            public override String getHelp()
            {

                return (
                    "Updates the specified user's forwarding settings (Multiple addresses)." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    "Specify a forwarding address to enable forwarding." + LF +
                    LF +
                    "No output if successful."
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "UserName  - the email address to update forwarding info for",
                        "Delete    - Delete the email after forwarding?",
                        "Forward   - destination email address(es) to forward to. Multipule addresses seporated by a semicolon",

                    }
                );

            }


            public override int function(String[] args)
            {


                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                string[] forwards = (args[2]).Split( new String[] {";",","} , StringSplitOptions.RemoveEmptyEntries );

                p("Updating "+forwards.Length +" forwards to URL [" + a.Url + "]");

                SvcUserAdmin.GenericResult genericResult = a.UpdateUserForwardingInfo2( username, password, args[0], args[1].Equals("Y") || args[1].Equals("y"), forwards  );

                if (!genericResult.Result)
                {

                    throw new Exception(genericResult.Message);

                }

                p("Completed successfully");

                return 1;

            }

        }


        class UserAutoResponseCommand : Command
        {

            public override String getCommand()
            {
                return "USERAUTO";
            }

            public override String getHelp()
            {

                return (
                    "Get auto response info for a specific user." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    LF +
                    "Outputs line containing the fields..." + LF +
                    "Address AutoResponseEnabled"+LF+
                    LF +
                    "Nothing will be output if the specified user is not found."

                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "UserName - the email address to get autoresponse info for"

                    }
                );

            }


            public override int function(String[] args)
            {


                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Fetchcing user from URL [" + a.Url + "]");

                SvcUserAdmin.UserAutoResponseResult r = a.GetUserAutoResponseInfo(username, password, args[0]);

                if (!r.Result)
                {
                    throw new Exception(r.Message);
                }

                if (r != null)
                {

                    p("Processing result...");

                    output(quotedList(args[0], r.Enabled.ToString()));


                }
                else
                {

                    p("User not found.");

                }

                p("Completed successfully");

                return 1;

            }

        }


        class GetUserCommand : Command
        {

            public override String getCommand()
            {
                return "GETUSER";
            }

            public override String getHelp()
            {

                return (
                    "Get info for a specific user." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    LF +
                    "Outputs line containing the fields..." + LF +
                    "UserName FirstName LastName Password IsDomainAdmin IsSystemAdmin" + LF +
                    LF +
                    "Nothing will be output if the specified user is not found."

                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "UserName - the email address to get info for"

                    }
                );

            }


            public override int function(String[] args)
            {


                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Fetchcing user from URL [" + a.Url + "]");

                SvcUserAdmin.UserInfoResult r = a.GetUser( username, password, args[0]);

                if (!r.Result)
                {
                    throw new Exception(r.Message);
                }

                p("Processing result...");

                output(quotedList( r.UserInfo.UserName, r.UserInfo.FirstName , r.UserInfo.LastName , r.UserInfo.Password , r.UserInfo.IsDomainAdmin ? "Y" : "N" , r.UserInfo.IsSystemAdmin ? "Y" : "N" ));


                p("Completed successfully");

                return 1;

            }

        }



        class DeleteUserCommand : Command
        {

            public override String getCommand()
            {
                return "DELETEUSER";
            }

            public override String getHelp()
            {

                return (
                    "Delete a specific user." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    LF +
                    "No output if successfull. No error if user not found."

                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "UserName - the username to delete",
                        "Domain - the domain of the username to delete"

                    }
                );

            }


            public override int function(String[] args)
            {


                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Deleting user with URL [" + a.Url + "]");

                SvcUserAdmin.GenericResult r = a.DeleteUser(username, password, args[0], args[1]);

                
                if (!r.Result)
                {
                    throw new Exception(r.Message);
                }

                
                p("Completed successfully.");

                return 1;

            }

        }



        class AddUserCommand : Command
        {

            public override String getCommand()
            {
                return "ADDUSER";
            }

            public override String getHelp()
            {

                return (
                    "Add a new user." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    LF +
                    "No output if successfull."

                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "UserName - new user name" ,
                        "Password - Password for new user" , 
                        "Domain - domain to add user into" ,
                        "FirstName - new user's first name" ,
                        "LastName - new user's last name",
                        "IsDomainAdmin - is this user a domain admin (Y=new user is a domain admin)"
                    }
                );

            }


            public override int function(String[] args)
            {



                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Adding user with URL [" + a.Url + "]");

                SvcUserAdmin.GenericResult r = a.AddUser(username, password, args[0], args[1], args[2], args[3], args[4], args[5][0]=='Y' || args[5][0]=='y'  );

                if (!r.Result)
                {
                    throw new Exception(r.Message);
                }

                p("Completed successfully");

                return 1;

            }

        }

        class AddAliasCommand : Command
        {

            public override String getCommand()
            {
                return "ADDALIAS";
            }

            public override String getHelp()
            {

                return (
                    "Add new alias(es)." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    LF +
                    "No output if successfull."

                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {
                        "Domain - domain to add alias into" ,
                        "Alias - alias name" ,
                        "Address(s) - new destination address, or addresses seporated by semicolons",
                    }
                );

            }


            public override int function(String[] args)
            {



                SvcAliasAdmin.svcAliasAdmin a = new TSMADump.SvcAliasAdmin.svcAliasAdmin();
                
                a.Url = baseURL + "/Services/svcAliasAdmin.asmx";

                p("Adding user with URL [" + a.Url + "]");

                String[] sa = new String[1];

                sa = args[2].Split(';');

                SvcAliasAdmin.GenericResult r = a.AddAlias( username, password, args[0], args[1], sa );

                if (!r.Result)
                {
                    throw new Exception(r.Message);
                }

                p("Completed successfully");

                return 1;

            }

        }

        class UpdateUser2Command : Command
        {

            public override String getCommand()
            {
                return "UPDATEUSER2";
            }

            public override String getHelp()
            {

                return (
                    "Update user info." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    LF +
                    "No output if successfull."

                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "UserName - new user name" ,
                        "Password - Password for new user" , 
                        "FirstName - new user's first name" ,
                        "LastName - new user's last name",
                        "IsDomainAdmin - is this user a domain admin (Y=new user is a domain admin)",
                        "MaxMailboxSize - new user's max mailbox size in MB"
                    }
                );

            }


            public override int function(String[] args)
            {

                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Adding user with URL [" + a.Url + "]");

                SvcUserAdmin.GenericResult r = a.UpdateUser2( username, password, args[0], args[1], args[2], args[3], args[4][0] == 'Y' || args[4][0] == 'y' , int.Parse( args[5] ));

                
                if (!r.Result)
                {
                    throw new Exception(r.Message);
                }


                p("Completed successfully");

                return 1;

            }

        }

        class GetRequestedUserSettingsCommand : Command
        {

            public override String getCommand()
            {
                return "GETUSERSETTINGS";
            }

            public override String getHelp()
            {

                return (
                    "Gets the value of a specified setting(s) for a given user." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    LF +
                    "Outputs the requested setting(s) if successfull."

                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "EmailAddress- email address of requested user" ,
                        "Setting - List of one or more setting keys (names) to get. Multipule settings should be comma separated." ,
                    }
                );

            }


            public override int function(String[] args)
            {

                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Adding user with URL [" + a.Url + "]");


                char[] comma = { ',' };

                String[] requestedSettings = args[1].Split(comma);

                SvcUserAdmin.SettingsRequestResult r = a.GetRequestedUserSettings(username, password, args[0], requestedSettings);

                if (!r.Result)
                {
                    throw new Exception(r.Message);
                }

                List<String> extractedValues = new List<string>(); 
                
                foreach( String value in r.settingValues )
                {

                    extractedValues.Add(value.Split('=')[1]);
                }



                output( quotedList(   extractedValues.ToArray() ));

                p("Completed successfully");

                return 1;

            }

        }



        class AddUserCommand2 : Command
        {

            public override String getCommand()
            {
                return "ADDUSER2";
            }

            public override String getHelp()
            {

                return (
                    "Add a new user." + LF +
                    "Username/password provided must be requested user or domain or system admin." + LF +
                    LF +
                    "No output if successfull."

                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "UserName - new user name" ,
                        "Password - Password for new user" , 
                        "Domain - domain to add user into" ,
                        "FirstName - new user's first name" ,
                        "LastName - new user's last name",
                        "IsDomainAdmin - is this user a domain admin (Y=new user is a domain admin)",
                        "MaxMailboxSize - new user's max mailbox size in MB"
                    }
                );

            }


            public override int function(String[] args)
            {



                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Adding user with URL [" + a.Url + "]");

                SvcUserAdmin.GenericResult r = a.AddUser2(username, password, args[0], args[1], args[2], args[3], args[4], args[5][0] == 'Y' || args[5][0] == 'y', int.Parse(args[6]));

                if (!r.Result)
                {
                    throw new Exception(r.Message);
                }              


                p("Completed successfully");

                return 1;

            }

        }







        class AliasAddressCommand : Command
        {

            public override String getCommand()
            {
                return "ALIASADDRESS";
            }

            public override String getHelp()
            {

                return (
                    "Dump all the target email addresses for a given alias in a domain." + LF +
                    "Username/password provided must be either domain or system admin." + LF +
                    LF +
                    "Outputs line for each address listed in the alias, each line has the fields..." + LF +
                    "Alias TargetAddress"
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "Domain - the name of the domain or domain alias to list",
                        "AliasName - the name of the alias to list"

                    }
                );

            }


            public override int function(String[] args)
            {

                SvcAliasAdmin.svcAliasAdmin a = new TSMADump.SvcAliasAdmin.svcAliasAdmin();

                a.Url = baseURL + "/Services/svcAliasAdmin.asmx";

                p("Fetchcing alias addresses from URL [" + a.Url + "]");

                SvcAliasAdmin.AliasInfoResult aliasInfoResult = a.GetAlias(username, password, args[0] , args[1] );

                if (!aliasInfoResult.Result)
                {

                    throw new Exception(aliasInfoResult.Message);
                }

                if (aliasInfoResult != null)
                {

                    p("Processing " + aliasInfoResult.AliasInfo.Addresses.Length + " result records...");

                    foreach (String address in (aliasInfoResult.AliasInfo.Addresses))
                    {

                        output(quotedList( args[1], address));

                    }

                }
                else
                {

                    p("No alias found.");
                }

                p("Completed successfully");

                return 1;

            }

        }



        class AliasAdminCommand : Command
        {

            public override String getCommand()
            {
                return "ALIAS";
            }

            public override String getHelp()
            {

                return (
                    "Dump all aliases in a domain." + LF +
                    "Username/password provided must be either domain or system admin." + LF +
                    LF +
                    "Outputs line for each alias in the domain, each line has the fields..." + LF +
                    "Alias"
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "Domain - the name of the domain or domain alias to list"

                    }
                );

            }


            public override int function(String[] args)
            {

                SvcAliasAdmin.svcAliasAdmin a = new TSMADump.SvcAliasAdmin.svcAliasAdmin();

                a.Url = baseURL + "/Services/svcAliasAdmin.asmx";

                p("Fetchcing aliases from URL [" + a.Url + "]");

                SvcAliasAdmin.AliasInfoListResult aliasInfoListResult = a.GetAliases(username, password, args[0]);

                if (!aliasInfoListResult.Result)
                {
                    throw new Exception(aliasInfoListResult.Message);
                }

                if (aliasInfoListResult != null)
                {


                    p("Processing " + aliasInfoListResult.AliasInfos.Length + " result records...");

                    foreach (SvcAliasAdmin.AliasInfo ai in (aliasInfoListResult.AliasInfos))
                    {

                        output(quotedList( ai.Name));

                    }

                }
                else
                {

                    p("No aliases found.");
                }

                p("Completed successfully");


                return 1;

            }

        }


        class SetCatchAllCommand : Command
        {

            public override String getCommand()
            {
                return "SETCATCHALL";
            }

            public override String getHelp()
            {

                return (
                    "Set the catch all alias for a domain." + LF +
                    "Username/password provided must be either domain or system admin." + LF 
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "Domain - the name of the domain or domain alias to list",
                        "AliasName - the name of the catch-all alias"

                    }
                );

            }


            public override int function(String[] args)
            {

                SvcAliasAdmin.svcAliasAdmin a = new TSMADump.SvcAliasAdmin.svcAliasAdmin();

                a.Url = baseURL + "/Services/svcAliasAdmin.asmx";

                p("Fetchcing aliases from URL [" + a.Url + "]");

                SvcAliasAdmin.GenericResult aliasInfoListResult = a.SetCatchAll(username, password, args[0] , args[1] );

                if (!aliasInfoListResult.Result)
                {
                    throw new Exception(aliasInfoListResult.Message);
                }

                p("Completed successfully");

                return 1;

            }

        }




        class DomainAdminCommand : Command
        {

            public override String getCommand()
            {
                return "DOMAIN"; 
            }

            public override String getHelp()
            {

                return (
                    "Dump all domains on the host." + LF +
                    "Username/password provided must be system admin." + LF +
                    LF +
                    "Outputs line for each domain on the server, each line has the fields..." + LF +
                    "Domain"
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {
                        
                    }
                );

            }


            public override int function(String[] args)
            {


                SvcDomainAdmin.svcDomainAdmin a = new SvcDomainAdmin.svcDomainAdmin();

                a.Url = baseURL + "/Services/svcDomainAdmin.asmx";

                p("Fetchcing domains from URL [" + a.Url + "]");

                SvcDomainAdmin.DomainListResult domainListResult = a.GetAllDomains(username, password);

                if (!domainListResult.Result) {

                    throw new Exception(domainListResult.Message);

                }

                if (domainListResult.DomainNames != null)
                {

                    p("Processing " + domainListResult.DomainNames.Length + " result records...");

                    foreach (String d in (domainListResult.DomainNames))
                    {

                        output(quotedList(d));

                    }
                }
                else
                {

                    p("No domains found.");
                }

                p("Completed successfully");

                return 1;

            }

        }


        class DomainAliasCommand : Command
        {

            public override String getCommand()
            {
                return "DOMAINALIAS";
            }

            public override String getHelp()
            {

                return (
                    "Dump all domain aliases for a given domain." + LF +
                    "Username/password provided must be domain admin or system admin." + LF +
                    LF +
                    "Outputs line for each domain alias on the server, each line has the fields..." + LF +
                    "DomainAlias"
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "Domain - the name of the domain to ifnd the aliases of"
                        
                    }
                );

            }


            public override int function(String[] args)
            {


                SvcDomainAliasAdmin.svcDomainAliasAdmin a = new TSMADump.SvcDomainAliasAdmin.svcDomainAliasAdmin();

                a.Url = baseURL + "/Services/svcDomainAliasAdmin.asmx";

                p("Fetchcing domain aliases from URL [" + a.Url + "]");

                SvcDomainAliasAdmin.DomainAliasInfoListResult domainAliasListResult = a.GetAliases(username, password, args[0]);

                if (!domainAliasListResult.Result) {

                    throw new Exception(domainAliasListResult.Message);
                }

                p("Processing " + domainAliasListResult.DomainAliasNames.Length + " result records...");

                foreach (String d in (domainAliasListResult.DomainAliasNames ))
                {

                    output(quotedList(d));

                }

                p("Completed successfully");

                return 1;

            }

        }


        class AddDomainAliasCommand : Command
        {

            public override String getCommand()
            {
                return "ADDDOMAINALIAS";
            }

            public override String getHelp()
            {

                return (
                    "Add a domain alias for a given domain." + LF +
                    "Username/password provided must be domain admin or system admin." + LF 
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "Domain - the name of the domain to add alias to",
                        "Alias - new alias to add"
                    }
                );

            }




            public override int function(String[] args)
            {


                SvcDomainAliasAdmin.svcDomainAliasAdmin a = new TSMADump.SvcDomainAliasAdmin.svcDomainAliasAdmin();

                a.Url = baseURL + "/Services/svcDomainAliasAdmin.asmx";

                p("Adding domain alias "+ args[0] +":"+ args[1] +" with URL [" + a.Url + "]");

                SvcDomainAliasAdmin.GenericResult domainAddAliasResult = a.AddDomainAlias( username, password, args[0] , args[1] );

                if (!domainAddAliasResult.Result)
                {

                    throw new Exception(domainAddAliasResult.Message);
                }

                p("Completed successfully");

                return 1;

            }

        }


        class AddDomainExCommand : Command
        {

            public override String getCommand()
            {
                return "ADDDOMAINEX";
            }

            public override String getHelp()
            {

                return (
                    "Creates a new domain using the system's default domain settings." + LF +
                    "Username/password provided must be domain admin or system admin." + LF
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {
                        "DomainName                  - The name of the domain name to add, in the format 'example.com'.",
                        "Path                        - The full path of the location in which the domain data should be stored.",
                        "PrimaryDomainAdminUserName  - The username for the domain administrator.",
                        "PrimaryDomainAdminPassword  - The password for the domain administrator.",
                        "PrimaryDomainAdminFirstName - The first name for the domain administrator.",
                        "PrimaryDomainAdminLastName  - The last name for the domain administrator.",
                        "IP                          - The IP Address on which the domain should listen.",
                    }
                );

            }




            public override int function(String[] args)
            {

                SvcDomainAdmin.svcDomainAdmin a = new TSMADump.SvcDomainAdmin.svcDomainAdmin();

                a.Url = baseURL + "/Services/svcDomainAdmin.asmx";

                p("Adding domain " + args[0] + " with URL [" + a.Url + "]");

                SvcDomainAdmin.GenericResult domainAddDomainExResult = a.AddDomainEx(username, password, args[0], args[1], args[2], args[3], args[4], args[5] , args[6] );

                if (!domainAddDomainExResult.Result)
                {

                    throw new Exception(domainAddDomainExResult.Message);
                }

                p("Completed successfully");

                return 1;

            }

        }







        class DomainSettingCommand : Command
        {

            public override String getCommand()
            {
                return "DOMAINSETTING";
            }

            public override String getHelp()
            {

                return (
                    "Dump the specified setting for the specified domain." + LF +
                    "Username/password provided must be domain admin or system admin." + LF +
                    LF +
                    "Outputs the requested setting"
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "Domain  - the name of the domain" ,
                        "Setting - the name of the setting"
                        
                    }
                );

            }


            public override int function(String[] args)
            {


                SvcDomainAdmin.svcDomainAdmin a = new TSMADump.SvcDomainAdmin.svcDomainAdmin();

                a.Url = baseURL + "/Services/svcDomainAdmin.asmx";

                p("Fetchcing domain setting from URL [" + a.Url + "]");

                String[] requestedSettings = { args[1] };


                SvcDomainAdmin.SettingsRequestResult settingsRequestResult = a.GetRequestedDomainSettings( username, password, args[0] , requestedSettings );

                if (!settingsRequestResult.Result)
                {

                    throw new Exception( settingsRequestResult.Message);
                }


                output(quotedList( settingsRequestResult.settingValues[0] ));              

                p("Completed successfully");

                return 1;

            }



        }


        class UserSettingCommand : Command
        {

            public override String getCommand()
            {
                return "USERSETTING";
            }

            public override String getHelp()
            {

                return (
                    "Dump the specified setting for the specified user." + LF +
                    "Username/password provided must be domain admin or system admin or the requested user." + LF +
                    LF +
                    "Outputs the requested setting"
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "Address - email address of the user" ,
                        "Setting - the name of the setting"
                        
                    }
                );

            }


            public override int function(String[] args)
            {


                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Fetchcing domain setting from URL [" + a.Url + "]");

                String emailaddress = args[0];
                String[] requestedSettings = { args[1] , "username" };

                p("requesteduser="+emailaddress+" requestedsetting="+requestedSettings[0]);

                SvcUserAdmin.SettingsRequestResult settingsRequestResult = a.GetRequestedUserSettings(username, password, emailaddress , requestedSettings);

                if (!settingsRequestResult.Result)
                {

                    throw new Exception(settingsRequestResult.Message);
                }


                output(quotedList(settingsRequestResult.settingValues[0]));

                p("Completed successfully");

                return 1;

            }



        }


        class GetUserGroupsByUserCommand : Command
        {

            public override String getCommand()
            {
                return "GETUSERGROUPSBYUSER";
            }

            public override String getHelp()
            {

                return (
                    "Dump the all user group IDs in the specified domain containing the specified user." + LF +
                    "Username/password provided must be domain admin or system admin or the requested user." + LF +
                    LF +
                    "Outputs the requested setting"
                  );
            }

            public override String[] getArgs()
            {
                return (
                    new String[] {

                        "Domain - Name o fthe Domain" ,
                        "User - User name to dump"

                    }
                );

            }


            public override int function(String[] args)
            {


                SvcUserAdmin.svcUserAdmin a = new TSMADump.SvcUserAdmin.svcUserAdmin();

                a.Url = baseURL + "/Services/svcUserAdmin.asmx";

                p("Fetchcing user groups from URL [" + a.Url + "]");

                String domain = args[1];
                String user = args[1];

                p("DomainName=" +domain+ " UserName=" + user);


                SvcUserAdmin.UserGroupsResult userGroupsRequestResult= a.GetUserGroupsByUser( username, password, domain, user, false );

                if (!userGroupsRequestResult.Result)
                {

                    throw new Exception(userGroupsRequestResult.Message);
                }

                String[] groupIds = (from groupId in userGroupsRequestResult.UserGroups select groupId.guid).ToArray<String>();
                
                output(quotedList(groupIds));

                p("Completed successfully");

                return 1;

            }



        }


        static Command[] commands = {

            new UserAdminCommand(),
            new AliasAdminCommand(),
            new AliasAddressCommand(),
            new DomainAdminCommand(),
            new UserForwardCommand(),
            new DomainAliasCommand(),
            new UserAutoResponseCommand(),
            new GetUserCommand(),
            new AddUserCommand(),
            new AddUserCommand2(),
            new UpdateUser2Command(),
            new UpdateUserForwardCommand(),
            new UpdateUserForward2Command(),
            new DeleteUserCommand(),
            new AddAliasCommand(),
            new DomainSettingCommand(),
            new AddDomainExCommand(),
            new AddDomainAliasCommand(),
            new UserSettingCommand(),
            new SetCatchAllCommand(),
            new GetRequestedUserSettingsCommand(),
            new GetUserGroupsByUserCommand(),


        };


        static int Main(string[] args)
        {


            String outFilePath = null;

            bool quoteFlag = false;
            bool commaFlag = false;

            int arg = 0;

            while ( (arg < args.Length) && (args[arg].Length> 1) && (args[arg][0] == '/'))
            {
                switch ( Char.ToUpper( args[arg][1]) )
                {
                    case 'O':       // Write to outputfile

                        outFilePath = args[arg].Substring( 2 );
                        p("Output will be written to the file " + outFilePath);

                        break;

                    case 'Q':       // Use quotes

                        quoteFlag = true;
                        p("Output fields will be surrounded with quotes.");

                        break;

                    case 'C':       // Use commas

                        commaFlag = true;
                        p("Output fields will be seporated with commas.");

                        break;

                    case 'L':      // List all commands - undocumented


                        List<Command> sortedCommandList = new List<Command>( commands );

                        sortedCommandList.Sort();

                        foreach( Command c in sortedCommandList )
                        {
                            String name = c.getCommand();

                            p(c.getCommand());
                            p(new string('-', name.Length));
                            p("  "+c.getHelp());
                            p("");
                            p("  Additional parameters:");
                            foreach( String a in c.getArgs())
                            {
                                p("    " + a);
                            }
                            p("");

                        }

                        return(1);


                    default:

                        p("Unknown paramter: " + args[arg][1] + ". Try running with no args for help.");
                        return (0);

                }

                arg++;
   
            }


            if (arg  == args.Length)        
            {

                p("TSMADump (c) 2008 Josh Levine [http://josh.com/TSMA]");
                p("");
                p("A command line utility to dump information from a SmarterMail server.");
                p("");
                p("Syntax:");
                p("  TSMADump [/oOutputFile] [/c] [/q] command baseURL username password [params]");
                p("");
                p("Where:");
                p("  /oOutputFile will optionally write the results to the specified file");
                p("  /c           will optionally add commas between output fields");
                p("  /q           will optionally add quotes around output fields");
                p("");
                p("Possible values for 'command' are:");

                foreach (Command c in commands)
                {
                    p("  " + c.getCommand());
                }

                p("");
                p("Each command has a variable set of parameters.");
                p("");
                p("Try running:");
                p("  TSMADump command");
                p("for information on that command.");
                p("");
                p("Return codes:");
                p("  Errorlevel 1 - Success");
                p("  Errorlevel 0 - Failure");
                p("");
                p("Example usage:");
                p("  TSMADump /oUsers.txt /q USER http://mail.test.com test@test.com foo test.com");
                p("");
                p("...would create a list of all the users in the domain test.com on the server at");
                p("mail.test.com. The list would be written to a text file called users.txt.");
                p("");

                return (1);
            }


            // Print help on all commands...

            if (String.Equals(args[arg].ToUpper(), "ALL"))
            {
                foreach( Command c in commands)
                {
                    p("---");
                    c.printHelp();
                    p("");

                }

                return (3);
            }


            foreach (Command c in commands)
            {

                if (String.Equals( args[arg].ToUpper() , c.getCommand().ToUpper() ))
                {

                    p( "Command specified:"+c.getCommand() );

                    arg++;      // Skip command

                    if (arg == args.Length)  // If no args specified then 
                    {  // Get help on specified command

                        c.printHelp();

                        return (3);


                    }

                    if (arg + 3 > args.Length)
                    {

                        p("Not enough args specified. Try running without any args for help.");
                        return (1);

                    }

                    c.setup( args[arg] , args[arg+1]  , args[arg+2] ,  quoteFlag, commaFlag , outFilePath);

                    arg += 3;   //Skip base, user, pass

                    if (arg + c.getArgs().Length != args.Length)
                    {

                        p("The "+c.getCommand()+" command requires "+c.getArgs().Length+" extra parameters.");
                        p("You specified " + ( args.Length - arg ));
                        p("Try running without any args for help.");
                        return (1);

                    }

                    string[] subArgs = new string[args.Length - arg ];

                    Array.ConstrainedCopy(args, arg , subArgs, 0 , subArgs.Length); 

                    p("Executing command...");

                    return (c.execute(subArgs));

                }

            }


            p("Invalid command:" + args[arg]);
            p("Try ruinning with no args for a list of valid commmands.");


            return (0);

        }
    }
}

