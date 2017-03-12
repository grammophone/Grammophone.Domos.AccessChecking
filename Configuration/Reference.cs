using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grammophone.Domos.AccessChecking.Configuration
{
	/// <summary>
	/// Reference to an entity via its code name.
	/// </summary>
	[Serializable]
	public class Reference
	{
		/// <summary>
		/// The code name of the entity.
		/// </summary>
		[Required]
		public string CodeName { get; set; }
	}
}
