using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.AccessChecking
{
	/// <summary>
	/// Contract for loading a <see cref="Configuration.PermissionsSetup"/>.
	/// </summary>
	public interface IPermissionsSetupProvider
	{
		/// <summary>
		/// Load the <see cref="Configuration.PermissionsSetup"/>.
		/// </summary>
		Configuration.PermissionsSetup Load();
	}
}
