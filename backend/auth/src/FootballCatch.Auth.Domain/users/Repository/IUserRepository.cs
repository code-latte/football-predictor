using System;

namespace FootballCatch.Auth.Domain.users.Repository;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);
    // En estas implementaciones, tengo que coger el objeto entero, incluyendo los dispositivos y los proveedores vinculados
    Task<User?> FindByExternalLoginAsync(string provider,string providerUserId, CancellationToken cancellationToken);
    Task<User?> FindByEmailAsync(string Email, CancellationToken cancellationToken);
    // No necesito update por que por lo visto cuando haces SaveChangesAsync EFCore automaticamente guarda los cambios de las entidades que
    // detecta que han cambiado

    // Task UpdateAsync(User user, CancellationToken cancellationToken);

}
