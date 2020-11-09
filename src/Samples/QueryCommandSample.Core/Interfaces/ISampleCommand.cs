using System;

namespace QueryCommandSample.Core.Interfaces
{
	public interface ISampleCommand
	{
		public Guid CommandId { get; set; }
	}
}