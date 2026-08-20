using System.Reflection;
using Ticketero.Domain.Entities;

namespace Ticketero.Tests;

public static class EntityTestExtensions
{
    public static TEntity WithId<TEntity>(this TEntity entity, int id) where TEntity : EntityBase
    {
        var prop = typeof(EntityBase).GetProperty(nameof(EntityBase.Id))
            ?? throw new InvalidOperationException("Propiedad Id no encontrada");
        prop.SetValue(entity, id);
        return entity;
    }
}