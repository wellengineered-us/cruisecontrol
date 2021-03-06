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

namespace WellEngineered.CruiseControl.PrivateBuild.NVelocity.Runtime.Parser.Node
{
	/// <summary> Handles modulus division<br><br>
    /// 
    /// Please look at the Parser.jjt file which is
    /// what controls the generation of this class.
    /// 
    /// </summary>
    /// <author>  <a href="mailto:wglass@forio.com">Will Glass-Husain</a>
    /// </author>
    /// <author>  <a href="mailto:pero@antaramusic.de">Peter Romianowski</a>
    /// </author>
    /// <author>  <a href="mailto:geirm@optonline.net">Geir Magnusson Jr.</a>
    /// </author>
    /// <version>  $Id: ASTModNode.java 691048 2008-09-01 20:26:11Z nbubna $
    /// </version>
    public class ASTModNode : ASTMathNode
    {
        /// <param name="id">
        /// </param>
        public ASTModNode(int id)
            : base(id)
        {
        }

        /// <param name="p">
        /// </param>
        /// <param name="id">
        /// </param>
        public ASTModNode(Parser p, int id)
            : base(p, id)
        {
        }

        public override Object Perform(Object left, Object right, IInternalContextAdapter context)
        {
            Type maxType = MathUtils.ToMaxType(left.GetType(), right.GetType());

            if (maxType == null)
            {
                return null;
            }

            return MathUtils.Modulo(maxType, left, right);
        }
    }
}