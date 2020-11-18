[![Project Status](http://opensource.box.com/badges/active.svg)](http://opensource.box.com/badges)

Smartsheet and Acumatica Cloud ERP Integration
==================================

Smartsheet is a modern SaaS visual project scheduling tool for tasks and resources that’s integrated with Acumatica Project Accounting to make any project professional’s job easier. Field supervisors and project managers can use Smartsheet to reschedule tasks and resources quickly and easily. Scheduled changes are sent back to Acumatica to update Project Accounting. Users who perform back-office business processes such as project costing, purchasing, and expense processing can do so in Acumatica.

Note: Smartsheet account is required to use this integration. 

For pricing please visit: [Smartsheet Pricing](https://www.smartsheet.com/pricing)

### Prerequisites

* Acumatica 2017 R2 or higher

Getting Started
-----------

### Install Smartsheet Customization Project
1. Download AcumaticaSmartsheetIntegration.zip from this repository
2. In your Acumatica ERP instance, import this as a customization project
3. Publish the customization project

    ![Screenshot](/_ReadMeImages/Image1.png)

### Configure SmartSheet/Acumatica Integration
Next step is to configure the connection between Acumatica and Smartsheet. This will be typically done by the administrator once for the Acumatica ERP instance.

1. Login to [Smartsheet](http://www.smartsheet.com)
2. Complete [Registration](http://developers.smartsheet.com/register)
3. From the Account menu, select “Developer Tools”

    ![Screenshot](/_ReadMeImages/Image2.png)

4. From the Developer Tools screen, select “Create New App”. 
   
   *  Specify the App Name
   *  App description
   *	App URL: Enter the link to your Acumatica ERP site in the following format:  
      https://[full URL of your Acumatica ERP site]/      
   *	App Contact: Provide an email for support
   *	App Redirect URL: Enter the link to your Acumatica ERP site in the following format:      
      https://[full URL of your Acumatica ERP site]/OAuthAuthenticationHandlerSS 
   *	Leave the “Publish App” unchecked
   *	Uploading a logo image is optional

    ![Screenshot](/_ReadMeImages/Image3.png)

5. Click Save, and Smartsheet will give a confirmation with App client ID and App Secret.

    ![Screenshot](/_ReadMeImages/Image4.png)
  
6. Please save the App Client ID and App secret. You will need this information to complete the connection within Acumatica. 
7. Login to your Acuamtica instance and search for Project Preferences Screen (PM101000).
8. Please provide the “App Client ID” and “App Secret” to the SmartSheet settings as show below

    ![Screenshot](/_ReadMeImages/Image5.png)

9. Save it and the connection between your Acumatica instance and Smartsheet is now setup. Acumatica users can now start using the Smartsheet integration.

### User Setup on Acumatica

With the connection information established in above Step, every Acumatica user that want to take advantage of the Smartsheet integration will need to get an access token to validate their identity. Here are the steps to get the access token:

1. Log into Acumatica instance.
2. Select “My Profile” from the user account info in the top right corner of the menu
3. This will bring up the “User Profile” screen with the newly added “Smartsheet Settings”.

    ![Screenshot](/_ReadMeImages/Image6.png)

4. Click on “GET SMARTSHEET TOKEN” button to receive a token from Smartsheet.
5. This will popup a window (as show below) to allow access between these two applications. 

    ![Screenshot](/_ReadMeImages/Image7.png)
  
6. Once allowed, your Smartsheet and Acumatica integration is ready to use.

    ![Screenshot](/_ReadMeImages/Image8.png)

This integration utilizes open source [Smartsheet SDK](https://github.com/smartsheet-platform/smartsheet-csharp-sdk)

For more information on how to use this integration to synchronize project information, please watch this [demo video](https://youtu.be/tkeiSayV9x0)

For FAQs and additional information, please visit [Acumatica-Smartsheet-Integration](https://www.acumatica.com/extensions/acumatica-smartsheet-integration)

Known Issues
------------
None at the moment.

## Copyright and License

Copyright © `2020` `Acumatica`

This component is licensed under the MIT License, a copy of which is available online [here](LICENSE.md)
