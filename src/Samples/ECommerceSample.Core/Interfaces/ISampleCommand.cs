using System;

namespace ECommerceSample.Core.Interfaces
{
	public interface ISampleCommand
	{
		public Guid CommandId { get; set; }
	}
}