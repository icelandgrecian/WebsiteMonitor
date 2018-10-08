# WebsiteMonitor

Simple C# app that scans URLs and emails if there are issues connecting to the sites. The lists of sites are contained in the file sites.txt

App.config settings:

Email

SMTP server to send emails, the from and to email addresses to use

    <add key="SMTPServer" value="mysmtpserver.com"/>
    <add key="EmailTo" value="myemail@domain.com"/>
    <add key="EmailFrom" value="myemail@domain.com"/>
    
Output directory to write log files
    
    <add key="OutputDirectory" value="output"/>
    
 Time how often to poll each url
 
    <add key="WaitTime" value="60000"/>
    
 How long a timeout on a site is before reporting an error
    
    <add key="TimeoutSeconds" value="5"/>    
  
 Time periods to skip monitoring
 
    <add key="SkipTimes" value="Sunday-00:00-06:00|Saturday-00:00-06:00|Monday-00:00-03:00|Tuesday-00:00-03:00|Wednesday-00:00-03:00|Thursday-00:00-03:00|Friday-00:00-03:00"/>
