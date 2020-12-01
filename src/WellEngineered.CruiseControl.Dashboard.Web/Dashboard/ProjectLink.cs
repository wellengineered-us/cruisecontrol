using WellEngineered.CruiseControl.Core.Reporting.Dashboard.Navigation;

namespace WellEngineered.CruiseControl.WebDashboard.Dashboard
{
	public class ProjectLink : GeneralAbsoluteLink
	{
		private readonly IProjectSpecifier buildSpecifier;
		private readonly string action;
		private readonly ICruiseUrlBuilder urlBuilder;
		public readonly string absoluteUrl;

		public ProjectLink(ICruiseUrlBuilder urlBuilder, IProjectSpecifier projectSpecifier, string text, string action)
			: base (text)
		{
			this.urlBuilder = urlBuilder;
			this.action = action;
			this.buildSpecifier = projectSpecifier;
		}

		public override string Url
		{
			get { return this.urlBuilder.BuildProjectUrl(this.action, this.buildSpecifier); }
		}
	}
}
