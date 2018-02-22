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
using System.Runtime.Serialization;

namespace SNTON.WebServices
{
    public class ResponseBase
    {
        public ResponseError Error { get; set; }

        /// <summary>
        /// Helper function to easily return general error message in case 
        /// of an unexpected exception. 
        /// </summary>
        /// <param name="e">Unhandled Exception caught</param>
        /// <returns>Response object of given type with error and arguments set.</returns>
        public static T GetResponseByException<T>(Exception e) where T : ResponseBase, new()
        {
            var ret = new T();
            ret.Error = new ResponseError();
            if (e != null && !string.IsNullOrWhiteSpace(e.Message))
            {
                if (e.InnerException != null && !string.IsNullOrWhiteSpace(e.InnerException.Message))
                {
                    ret.Error.Arguments = new string[2];
                    ret.Error.Message = Constants.ErrorMessages.ExceptionOccured_P2;
                    ret.Error.Arguments[1] = e.InnerException.Message;
                }
                else
                {
                    ret.Error.Arguments = new string[1];
                    ret.Error.Message = Constants.ErrorMessages.ExceptionOccured_P1;
                }
                ret.Error.Arguments[0] = e.Message;
            }
            return ret;
        }


    }
    public class ResponseDataBase : ResponseBase
    {
        public List<string> data { get; set; } = new List<string>();
    }
}
