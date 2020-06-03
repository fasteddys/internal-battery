using System;
using System.Collections.Generic;
using System.Text;

namespace UpDiddyLib.Dto
{
    public class EmploymentDto
	{
		/// <summary>
		/// Employment start date.
		/// </summary>
		public DateTime? StartDate { get; set; }

		/// <summary>
		/// Employment end date.
		/// </summary>
		public DateTime? EndDate { get; set; }

		/// <summary>
		/// If 1, this is current employment.
		/// </summary>
		public int IsCurrent { get; set; }

		/// <summary>
		/// Job title associated with employment.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Job description or job responsibilities.
		/// </summary>
		public string JobDescription {get; set;}

		/// <summary>
		/// Employment company's guid.
		/// </summary>
		public Guid? CompanyGuid { get; set; }
    }
}
