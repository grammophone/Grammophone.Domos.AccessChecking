using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Configuration;
using Grammophone.Domos.AccessChecking.Configuration;

namespace Grammophone.Domos.AccessChecking
{
	/// <summary>
	/// Loads a <see cref="Configuration.PermissionsSetup"/> from
	/// a XAML file specified in a <see cref="XamlSettingsSection"/>.
	/// </summary>
	public class XamlPermissionsSetupProvider : IPermissionsSetupProvider
	{
		#region Private fields

		private string xamlConfigurationSectionName;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="xamlConfigurationSectionName">
		/// The name of the configuration sectin of type <see cref="XamlSettingsSection"/>
		/// specifying the XAML file of a <see cref="PermissionsSetup"/> instance.
		/// </param>
		public XamlPermissionsSetupProvider(string xamlConfigurationSectionName)
		{
			if (xamlConfigurationSectionName == null) throw new ArgumentNullException(nameof(xamlConfigurationSectionName));

			this.xamlConfigurationSectionName = xamlConfigurationSectionName;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Load the <see cref="Configuration.PermissionsSetup"/> from the 
		/// XAML file specified in the constructor.
		/// </summary>
		public PermissionsSetup Load()
		{
			var configuration = new XamlConfiguration<PermissionsSetup>(xamlConfigurationSectionName);

			return configuration.Settings;
		}

		#endregion
	}
}
