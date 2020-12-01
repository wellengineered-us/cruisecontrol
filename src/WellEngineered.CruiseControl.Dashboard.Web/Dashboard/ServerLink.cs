using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class ServerLink : GeneralAbsoluteLink
	{
		private readonly IServerSpecifier serverSpecifier;
		private readonly string action;
		private readonly ICruiseUrlBuilder urlBuilder;
		public readonly string absoluteUrl;

		public ServerLink(ICruiseUrlBuilder urlBuilder, IServerSpecifier serverSpecifier, string text, string action)
			: base (text)
		{
			this.urlBuilder = urlBuilder;
			this.action = action;
			this.serverSpecifier = serverSpecifier;
		}

		public override string Url
		{
			get { return this.urlBuilder.BuildServerUrl(this.action, this.serverSpecifier); }
		}
	}
}
