// Copyright (c) 2016 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.

using System;
using System.Collections.Generic;
using System.Linq;
using SNTON.BusinessLogic.UserInterfaceBackend;
using SNTON.Constants;
using SNTON.WebServices;
//using SNTON.WebServices.UserInterfaceBackend.Models;
using SNTON.WebServices.UserInterfaceBackend.Requests;
using SNTON.WebServices.UserInterfaceBackend.Responses;
using NHibernate;
using NHibernate.Util;
using VI.MFC.Logging; 
using System.Linq.Expressions;
using System.Text;

using SNTON.Components.LockManager;

namespace SNTON.BusinessLogic
{

    /// <summary>
    /// Business logic interface for the user interface backend goes here.
    /// Basically, these methods will be called primarily by the RESTful service layer.
    /// </summary>
    public partial class BusinessLogic : IUserInterfaceBackend
    {
        #region Properites

        #endregion
        #region Helper methods
        #endregion
        

        public ResponseDataBase TTT()
        {
            //this.systemParametersConfigurationProvider
            ResponseDataBase b = new ResponseDataBase();
            return b;
        }
        
    }

}

