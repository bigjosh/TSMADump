TSMADump
========

Full project info at...
http://josh.com/tsma/tsmadump/

Tools For SmarterMail Admins

SMADump - A batch file utility for SmarterMail administrators
Overview

TSMADump is a command-line utility that lets you retrieve information about domains, users, auto-responders, and aliases from a SmarterMail server. It outputs plain text files that are perfect for use in batch files.
Download

Click here to download TSMADump.zip.

Note that you need to have Microsoft .NET 2.0 installed to run TSMADUMP.EXE. All SmarterMail servers already have this installed because SmarterMail needs it too. If you want to run TSMADUMP on a different machine, you can install .NET 2.0 though Windows Update or download the .NET 2.0 Redistributable directly from Microsoft.
Features

    Easily export a list of all the users, aliases, domains, or domain aliases on a SmarterMail server.
    Output to a text file for processing in a batch file or importing into a spreadsheet or database.
    Can be run directly on the SmarterMail server or on any Windows machine that can access the server's webmail homepage.
    Uses SmarterMail's documented and reliable Web Services interface rather than scraping HTML or XML files.

Example Uses

    Create a spreadsheet with all the users and their respective info. 
    Find all users who are forwarding their email to Yahoo accounts.
    Send an email to all the domain admins on a server.
    Automatically create new users

(All example batch files are included in the above zip file)
Sample command lines

TSMADUMP DOMAIN http://webmail.test.com admin foobar

Will list all of the domains on the SmarterMail server whose login page is located at webmail.test.com. Note that "admin" must be an authorized host administrator, and "foobar" would be the password for that account. 

 

TSMADUMP /Q /OUsers.csv USER http://webmail.test.com  me@test.com foobean test.com

Will create a file called Users.csv that lists all of the users in the domain josh.com and can be opened directly in Excel. The Note that me@test.com is the domain admin.

 

TSMADUMP ADDUSER http://webmail.test.com  admin@josh.com foobean jsmith please test.com Joe Smith N

Will add a new user account "jsmith@test.com" with the password "please", first name "Joe", last name "Smith", and he will not have domain admin rights.

 
