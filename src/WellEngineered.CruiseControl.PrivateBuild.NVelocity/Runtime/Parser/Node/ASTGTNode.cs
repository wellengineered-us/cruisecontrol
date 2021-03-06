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

using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Context;
using WellEngineered.CruiseControl.PrivateBuild.NVelocity.Exception;

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node
{
	/// <summary> Handles arg1 &gt; arg2<br><br>
    /// 
    /// Only subclasses of Number can be compared.<br><br>
    /// 
    /// Please look at the Parser.jjt file which is
    /// what controls the generation of this class.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:wglass@forio.com">Will Glass-Husain</a>
    /// </author>
    /// <author>  <a href="mailto:pero@antaramusic.de">Peter Romianowski</a>
    /// </author>

    public class ASTGTNode : SimpleNode
    {
        /// <param name="id">
        /// </param>
        public ASTGTNode(int id)
            : base(id)
        {
        }

        /// <param name="p">
        /// </param>
        /// <param name="id">
        /// </param>
        public ASTGTNode(Parser p, int id)
            : base(p, id)
        {
        }


        /// <seealso cref="SimpleNode.jjtAccept(org.apache.veloobjectser.node.ParserVisitor, java.lang.Object)">
        /// </seealso>
        public override object Accept(IParserVisitor visitor, object data)
        {
            return visitor.Visit(this, data);
        }

        /// <seealso cref="SimpleNode.evaluate(org.apache.velocity.context.InternalContextAdapter)">
        /// </seealso>
        public override bool Evaluate(IInternalContextAdapter context)
        {
            /*
            *  get the two args
            */

            object left = this.GetChild(0).Value(context);
            object right = this.GetChild(1).Value(context);

            /*
            *  if either is null, lets log and bail
            */

            if (left == null || right == null)
            {
                System.String msg = (left == null ? "Left" : "Right") + " side (" + this.GetChild((left == null ? 0 : 1)).Literal + ") of '>' operation has null value at " + Log.Log.FormatFileString(this);

                if (this.rsvc.GetBoolean(RuntimeConstants.RUNTIME_REFERENCES_STRICT, false))
                {
                    throw new VelocityException(msg);
                }

                this.log.Error(msg);
                return false;
            }

            try
            {
                return ObjectComparer.CompareObjects(left, right) == ObjectComparer.Greater;
            }
            catch (ArgumentException argumentException)
            {
                this.log.Error(argumentException.Message);

                return false;
            }
        }

        /// <seealso cref="SimpleNode.value(org.apache.velocity.context.InternalContextAdapter)">
        /// </seealso>
        public override object Value(IInternalContextAdapter context)
        {
            bool val = this.Evaluate(context);

            return val ? true : false;
        }
    }
}