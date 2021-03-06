﻿using System;
using System.ComponentModel;

using WellEngineered.CruiseControl.Remote.Messages;

namespace WellEngineered.CruiseControl.Remote
{
    /// <summary>
    /// A message has been received.
    /// </summary>
    public class MessageReceivedEventArgs
        : AsyncCompletedEventArgs
    {
        #region Private fields
        private readonly Response response;
        #endregion

        #region Constructors
        /// <summary>
        /// Initialise a new <see cref="MessageReceivedEventArgs"/>.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="error"></param>
        /// <param name="cancelled"></param>
        /// <param name="userState"></param>
        public MessageReceivedEventArgs(Response response, Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            this.response = response;
        }
        #endregion

        #region Public properties
        #region Response
        /// <summary>
        /// The response message.
        /// </summary>
        public Response Response
        {
            get { return this.response; }
        }
        #endregion
        #endregion
    }
}
