// Copyright (c) 2009, 2010 Ni Technology
// All rights reserved.
//
// The copyright to the computer program(s) herein is the property of
// Ni Technology. The program(s) may be used and/or copied
// only with the written permission of the owner or in accordance with
// the terms and conditions stipulated in the contract under which the
// program(s) have been supplied.
//
// $Id: PLC_1.cs,v 1.3 2011/07/21 14:28:58 deld Exp $

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using log4net;
using VI.MFC.Constants;
using VI.MFC.COM;
using VI.MFC.Logging;
using VI.MFC.Logic;

namespace MFC.Scripts
{
    /// <summary>
    /// Sample PLC Script implementing IVIScriptLogic
    /// </summary>
    public class PLC_1 : IVIScriptLogic
    {
        private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Internal methods only.

        /// <summary>
        /// Will be called everytime the script has been reloaded/recompiled.
        /// </summary>
        /// <param name="theNode">XML Node containingt the config information.</param>
        /// <returns>
        /// true-  Init was successful. Script can be used.
        /// false- Script failed. Script will not be called at all.
        /// </returns>
        public virtual bool Init(XmlNode theNode)
        {
            return true;
        }

        /// <summary>
        /// Will be called everytime the script is about to be unloaded/removed.
        /// </summary>
        public virtual void Exit()
        {
        }

        #endregion

        #region All your supporting methods and properties should go here...

        private int toteCounterInfeedStation;
        private int toteCounterWaitingPoint;
        private int toteCounterToteStartPos;

        /// <summary>
        /// Makes it easier to retrieve the most current field values
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="currentLocation">Extracted location from the neutriono</param>
        /// <param name="picValue">Extracted picValue from the neutriono</param>
        /// <param name="barcode">Extracted barcode from the neutriono</param>
        private void GetStandardFields(
                Neutrino dataIn,
                out string currentLocation,
                out string picValue,
                out string barcode)
        {
            byte[] vi = dataIn.GetRawField("SourceLocation");
            currentLocation = Encoding.GetEncoding(1252).GetString(vi);
            vi = dataIn.GetRawField("PIC");
            picValue = Encoding.GetEncoding(1252).GetString(vi);
            vi = dataIn.GetRawField("Barcode");
            barcode = Encoding.GetEncoding(1252).GetString(vi);
        }

        /// <summary>
        /// Easy solution to write fields to be sent back to the PLC
        /// </summary>
        /// <param name="theField">Key to be written</param>
        /// <param name="theValue">value to be used</param>
        /// <param name="dataOut">Neutrino to be worked on</param>
        private void WriteField(string theField, string theValue, ref Neutrino dataOut)
        {
            int byteCount = Encoding.GetEncoding(1252).GetByteCount(theValue);
            byte[] data = new byte[byteCount];
            Array.Copy(Encoding.GetEncoding(1252).GetBytes(theValue), data, byteCount);
            dataOut.AddField(theField, data);
        }

        private void CreateDestinationReply(
                string picValue,
                string barcode,
                string sourceLocation,
                string destinationLocation,
                out Neutrino dataOut)
        {
            dataOut = new Neutrino();

            WriteField(StandardTelegramFieldNames.TelegramId, "51", ref dataOut);
            WriteField("PIC", picValue, ref dataOut);
            WriteField("Barcode", barcode, ref dataOut);
            WriteField("SourceLocation", sourceLocation, ref dataOut);
            WriteField("DestinationLocation", destinationLocation, ref dataOut);
        }

        private void CreateWaitingPointReply(
                string picValue,
                string barcode,
                string sourceLocation,
                string destinationLocation,
                out Neutrino dataOut)
        {
            dataOut = new Neutrino();

            WriteField(StandardTelegramFieldNames.TelegramId, "52", ref dataOut);
            WriteField("PIC", picValue, ref dataOut);
            WriteField("Barcode", barcode, ref dataOut);
            WriteField("SourceLocation", sourceLocation, ref dataOut);
            WriteField("DestinationLocation", destinationLocation, ref dataOut);
        }

        private void CreateToteStartReply(
                string picValue,
                string barcode,
                string sourceLocation,
                string destinationLocation,
                out Neutrino dataOut)
        {
            dataOut = new Neutrino();

            WriteField(StandardTelegramFieldNames.TelegramId, "54", ref dataOut);
            WriteField("PIC", picValue, ref dataOut);
            WriteField("Barcode", barcode, ref dataOut);
            WriteField("SourceLocation", sourceLocation, ref dataOut);
            WriteField("DestinationLocation", destinationLocation, ref dataOut);
        }

        /// <summary>
        /// Returns the value of the specified field as a string
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="field">Key name for which data should be extracted from the neutrino</param>
        /// <returns>data found for given key</returns>
        private string GetField(Neutrino dataIn, string field)
        {
            return Encoding.GetEncoding(1252).GetString(dataIn.GetRawField(field));
        }

        #endregion

        #region Methods to be implemented by the user

        /// <summary>
        /// MFC State telegram request. MFC is requested to send it's state back to the PLC.
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="dataOut">The data to send back. If null then no data will be send back.</param>
        private void OnMFCState(Neutrino dataIn, out Neutrino dataOut)
        {
            dataOut = null;
        }

        /// <summary>
        /// PLC State telegram. PLC sends it's state to the MFC.
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="dataOut">The data to send back. If null then no data will be send back.</param>
        private void OnPLCState(Neutrino dataIn, out Neutrino dataOut)
        {
            dataOut = null;
        }

        /// <summary>
        /// Tote passed a scanner. PLC is waiting for destination location from MFC.
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="dataOut">The data to send back. If null then no data will be send back.</param>
        private void OnDestinationRequest(Neutrino dataIn, out Neutrino dataOut)
        {
            string currentLocation, picValue, barcode;

            dataOut = null;

            GetStandardFields(dataIn, out currentLocation, out picValue, out barcode);

            // Just an example. Please put your code here...
            if (currentLocation == "010201")
            {
                // Increment tote counter
                toteCounterInfeedStation++;

                string destinationLocation;
                if (toteCounterInfeedStation % 10 == 0)
                {
                    // We send every 10th tote to the premium quality check...
                    destinationLocation = "010501";
                }
                else
                {
                    if (barcode.EndsWith("1"))
                    {
                        // In case tote barcode ends by "1", we will send it to reject lane
                        destinationLocation = "010401";
                    }
                    else
                    {
                        // By default, all totes will go straight
                        destinationLocation = "010399";
                    }
                }
                CreateDestinationReply(picValue, barcode, currentLocation, destinationLocation, out dataOut);
            }
            // Feel free to check other locations as well...
        }

        /// <summary>
        /// Tote start telegram received.
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="dataOut">The data to send back. If null then no data will be send back.</param>
        private void OnRequestAtToteStart(Neutrino dataIn, out Neutrino dataOut)
        {
            string currentLocation, picValue, barcode;

            dataOut = null;

            GetStandardFields(dataIn, out currentLocation, out picValue, out barcode);

            // Just an example. Please put your code here...
            // Feel free to check other locations as well...
            if (currentLocation == "020201")
            {
                // Increment tote counter
                toteCounterToteStartPos++;

                string destinationLocation;
                if (toteCounterInfeedStation % 10 == 0)
                {
                    // We send every 10th tote to the premium quality check...
                    destinationLocation = "020501";
                }
                else
                {
                    if (barcode.EndsWith("1"))
                    {
                        // In case tote barcode ends by "1", we will send it to reject lane
                        destinationLocation = "040401";
                    }
                    else
                    {
                        // By default, all totes will go straight
                        destinationLocation = "090399";
                    }
                }
                CreateToteStartReply(picValue, barcode, currentLocation, destinationLocation, out dataOut);
            }
        }

        /// <summary>
        /// Tote passed a scanner. PLC is waiting for destination location from MFC
        /// while tote is stopped.
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="dataOut">The data to send back. If null then no data will be send back.</param>
        private void OnWaitingPointRequest(Neutrino dataIn, out Neutrino dataOut)
        {
            string currentLocation, picValue, barcode;

            dataOut = null;

            GetStandardFields(dataIn, out currentLocation, out picValue, out barcode);

            // Just an example. Please put your code here...
            if (currentLocation == "010202")
            {
                // Increment tote counter
                toteCounterWaitingPoint++;

                string destinationLocation;
                if (toteCounterWaitingPoint % 10 == 0)
                {
                    // We send every 10th tote to the premium quality check...
                    destinationLocation = "010502";
                }
                else
                {
                    if (barcode.EndsWith("1"))
                    {
                        // In case tote barcode ends by "1", we will send it to reject lane
                        destinationLocation = "010402";
                    }
                    else
                    {
                        // By default, all totes will go straight
                        destinationLocation = "010392";
                    }
                }
                CreateWaitingPointReply(picValue, barcode, currentLocation, destinationLocation, out dataOut);
            }
            // Feel free to check other locations as well...
        }

        private void OnSortationReport(Neutrino dataIn)
        {
            string currentLocation, picValue, barcode;

            GetStandardFields(dataIn, out currentLocation, out picValue, out barcode);
            string requestedDestination = GetField(dataIn, "RequestedDestination");
            string actualDestination = GetField(dataIn, "ActualDestination");
            string divertCode = GetField(dataIn, "DivertCode");

            if (actualDestination != requestedDestination)
            {
                logger.ErrorMethod("Wrong sortation of " + barcode + " at " + currentLocation + " detected.");
            }
        }

        /// <summary>
        /// Sortation report after a waiting point reply
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        private void OnSortationReportWaitingPoint(Neutrino dataIn)
        {
            // We just call the standard waiting point method. Feel free
            // to implement whatever you like...
            OnSortationReport(dataIn);
        }

        /// <summary>
        /// Sortation report after a tote start reply
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        private void OnSortationReportToteStart(Neutrino dataIn)
        {
            // We just call the standard waiting point method. Feel free
            // to implement whatever you like...
            OnSortationReport(dataIn);
        }

        /// <summary>
        /// PLC wants to log some data...
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        private void OnPLCLoggingInfo(Neutrino dataIn)
        {
            string theLogInfo = GetField(dataIn, "LoggingData");
            logger.InfoMethod(theLogInfo);
        }

        /// <summary>
        /// Client is requested to send it's watchdog
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="dataOut">The data to send back. If null then no data will be send back.</param>
        private void OnWatchdogRequest(Neutrino dataIn, out Neutrino dataOut)
        {
            dataOut = null;
            WriteField(StandardTelegramFieldNames.TelegramId, "99", ref dataOut);
        }

        /// <summary>
        /// Incoming watchdog reply from peer detected.
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="dataOut">The data to send back. If null then no data will be send back.</param>
        private void OnWatchdogReply(Neutrino dataIn, out Neutrino dataOut)
        {
            dataOut = null;
        }

        /// <summary>
        /// Tote has passed a message point
        /// </summary>
        /// <param name="dataIn">data to be analysed</param>
        private void OnMessagePointRequest(Neutrino dataIn)
        {
            // Put your code in here.
        }

        /// <summary>
        /// Will be called each time a telegram has arrived.
        /// </summary>
        /// <param name="theInstance">Communication Instance</param>
        /// <param name="dataIn">data to be analysed</param>
        /// <param name="dataOut">The data to send back. If null then no data will be send back.</param>
        public virtual void Process(string theInstance, Neutrino dataIn, out List<Neutrino> dataOut)
        {
            Neutrino theNeutrino = null;

            // Default: We will not send anything back...
            dataOut = null;

            // What kind of telegram came in?
            byte[] vi = dataIn.GetRawField(StandardTelegramFieldNames.TelegramId);
            logger.InfoMethod("Processing " + dataIn.TheName + ".");

            switch (Encoding.GetEncoding(1252).GetString(vi))
            {
                // Tote passed a scanner. PLC is waiting for destination location from MFC.
                case "01":
                    OnDestinationRequest(dataIn, out theNeutrino);
                    break;

                // Tote passed a waiting point scanner. PLC is waiting for destination location from MFC
                // while having the tote stop.
                case "02":
                    OnWaitingPointRequest(dataIn, out theNeutrino);
                    break;

                // Tote has passed a message point
                case "03":
                    OnMessagePointRequest(dataIn);
                    break;

                case "04":
                    OnRequestAtToteStart(dataIn, out theNeutrino);
                    break;

                // Tote has been sorted out after a destination request
                case "11":
                    OnSortationReport(dataIn);
                    break;

                // Tote has been sorted out after a waiting point request
                case "12":
                    OnSortationReportWaitingPoint(dataIn);
                    break;

                case "14":
                    OnSortationReportToteStart(dataIn);
                    break;

                // MFC State telegram request. MFC is requested to send it's state back to the PLC.
                case "61":
                    OnMFCState(dataIn, out theNeutrino);
                    break;

                case "81":
                    OnPLCLoggingInfo(dataIn);
                    break;

                // PLC reports it's state to the MFC.
                case "91":
                    OnPLCState(dataIn, out theNeutrino);
                    break;

                // Server sends back it's watchdog telegram
                case "98":
                    OnWatchdogReply(dataIn, out theNeutrino);
                    break;

                // Client should send it's watchdog telegram
                case "99":
                    OnWatchdogRequest(dataIn, out theNeutrino);
                    break;
            }
            if (theNeutrino != null)
            {
                dataOut = new List<Neutrino>();
                dataOut.Add(theNeutrino);
            }
        }

        #endregion
    }
}