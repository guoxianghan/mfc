// Copyright (c) 2009 - 2010 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.
//
// $Id: AssemblyInfo.cs,v 1.3 2011/07/21 14:28:58 deld Exp $

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using log4net.Config;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("MFC")]
[assembly: AssemblyDescription("Material Flow Controller Application")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ni Technology")]
[assembly: AssemblyProduct("Vision")]
[assembly: AssemblyCopyright("Copyright ©2009, 2014 Ni Technology.")]
[assembly: AssemblyTrademark("VISION.MFC Material Flow Controller Application.")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("3abcb52e-f2e3-43c9-b6b3-4e733609d794")]

// Setting for log4net to be able to change the current logging
// level without having to restart the MFC

[assembly: XmlConfigurator(Watch = true)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("7.1.0.0")]
[assembly: NeutralResourcesLanguage("en")]