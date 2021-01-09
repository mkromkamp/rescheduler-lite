using System;

namespace Rescheduler.Core.Entities
{
    public abstract class EntityBase
    {
        protected EntityBase() {}

        protected EntityBase(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}