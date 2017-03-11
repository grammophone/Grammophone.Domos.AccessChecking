using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grammophone.Domos.Domain.Workflow;

namespace Grammophone.Domos.AccessChecking.Configuration
{
	/// <summary>
	/// Depicts an allowed access to a <see cref="StatePath"/>.
	/// </summary>
	[Serializable]
	public class StatePathAccess
	{
		#region Primitive properties

		/// <summary>
		/// The <see cref="StatePath.CodeName"/> of
		/// the <see cref="StatePath"/>.
		/// </summary>
		public string StatePathCodeName { get; set; }

		#endregion
	}
}
