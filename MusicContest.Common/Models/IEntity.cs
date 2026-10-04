namespace MusicContest.Common.Models;

// IEntity: I = Interface (інтерфейс), Entity = сутність.
// Інтерфейс задає контракт: кожен клас, який реалізує IEntity,
// зобов'язаний мати властивість Id.
// Завдяки цьому універсальний CRUD-сервіс знає, що у будь-якого T є ідентифікатор.
public interface IEntity
{
    // Id = Identifier = ідентифікатор.
    // Guid (Globally Unique Identifier) — глобально унікальний ідентифікатор.
    // Він значно надійніший за прості 1, 2, 3, коли об'єкти можуть створюватися незалежно.
    Guid Id { get; set; }
}
