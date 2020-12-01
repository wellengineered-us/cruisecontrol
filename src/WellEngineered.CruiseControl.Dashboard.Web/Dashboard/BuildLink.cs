using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class BuildLink : GeneralAbsoluteLink
	{
		private readonly IBuildSpecifier buildSpecifier;
		private readonly string action;
		private readonly ICruiseUrlBuilder urlBuilder;
		public readonly string absoluteUrl;

		public BuildLink(ICruiseUrlBuilder urlBuilder, IBuildSpecifier buildSpecifier, string text, string action)
			: base (text)
		{
			this.urlBuilder = urlBuilder;
			this.action = action;
			this.buildSpecifier = buildSpecifier;
		}

		public override string Url
		{
			get { return this.urlBuilder.BuildBuildUrl(this.action, this.buildSpecifier); }
		}
	}
}
