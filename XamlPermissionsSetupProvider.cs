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
	/// a XAML file.
	/// </summary>
	public class XamlPermissionsSetupProvider : IPermissionsSetupProvider
	{
		#region Private fields

		private string xamlFilename;

		#endregion

		#region Construction

		/// <summary>
		/// Create.
		/// </summary>
		/// <param name="xamlFilename">
		/// The name of the XAML file of a <see cref="PermissionsSetup"/> instance.
		/// </param>
		public XamlPermissionsSetupProvider(string xamlFilename)
		{
			if (xamlFilename == null) throw new ArgumentNullException(nameof(xamlFilename));

			this.xamlFilename = xamlFilename;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Load the <see cref="Configuration.PermissionsSetup"/> from the 
		/// XAML file specified in the constructor.
		/// </summary>
		public PermissionsSetup Load()
		{
			return XamlConfiguration<PermissionsSetup>.LoadSettings(xamlFilename);
		}

		#endregion
	}
}
