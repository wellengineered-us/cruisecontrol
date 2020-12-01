/*
* Licensed to the Apache Software Foundation (ASF) under one
* or more contributor license agreements.  See the NOTICE file
* distributed with this work for additional information
* regarding copyright ownership.  The ASF licenses this file
* to you under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
*   http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing,
* software distributed under the License is distributed on an
* "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* KIND, either express or implied.  See the License for the
* specific language governing permissions and limitations
* under the License.    
*/

using System;
using System.Runtime.Serialization;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception
{
	/// <summary>  
    /// Base class for Velocity exceptions thrown to the
    /// application layer.
    /// </summary>
    [Serializable]
    public class VelocityException : System.Exception
    {
        public VelocityException(String exceptionMessage)
            : base(exceptionMessage)
        {
        }

        public VelocityException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        protected VelocityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}