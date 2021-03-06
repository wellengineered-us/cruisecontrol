using System.Collections;

using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Attributes;
using WellEngineered.CruiseControl.PrivateBuild.NetReflector.Util;

namespace WellEngineered.CruiseControl.PrivateBuild.NetReflector.Serialisers
{
	public class DefaultSerialiserFactory : ISerialiserFactory
	{
		public IXmlMemberSerialiser Create(ReflectorMember member, ReflectorPropertyAttribute attribute)
		{
			if (member.MemberType.IsArray)
			{
				return new XmlArraySerialiser(member, attribute);
			}
			else if (typeof(ICollection).IsAssignableFrom(member.MemberType) || 
				(attribute.InstanceType != null && typeof(ICollection).IsAssignableFrom(attribute.InstanceType)))
			{
				return new XmlCollectionSerialiser(member, attribute);
			}
			return new XmlMemberSerialiser(member, attribute);
		}
	}
}