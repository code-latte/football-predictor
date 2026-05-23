using System;

using FootballCatch.Auth.Domain.Users;
using FootballCatch.Auth.Domain.Users.Aggregates;
using FootballCatch.Auth.Domain.Users.Repository;
using FootballCatch.Auth.Domain.Users.Services;
using FootballCatch.Common.BuildingBlocks.Persistance;
using FootballCatch.Common.Mediator;
using FootballCatch.Common.Types;

namespace FootballCatch.Auth.Application.Users.Commands.GoogleLogin;

public sealed class GoogleLoginCommandHandler : ICommandHandler<GoogleLoginCommand, GoogleLoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IClock _clock;
    private readonly IUnitOfWork _uow;

    public GoogleLoginCommandHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, IClock clock, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _clock = clock;
        _uow = uow;
    }

    public async Task<GoogleLoginResult> HandleAsync(GoogleLoginCommand command, CancellationToken ct = default)
    {
        try
        {      
            // si encuentra al user con este metodo, es que se ha registrado con google alguna vez ya
            User? existingUser=await _userRepository.FindByExternalLoginAsync(command.Provider,command.ProviderUserId,ct);
            if(existingUser is null)
            {  
                // Si lo encuentra aqui es que nunca se ha registrado con google pero si con usuario y contraseña
                existingUser=await _userRepository.FindByEmailAsync(command.Email, ct);
                // En caso de que sea la primera vez que se loggee
                if(existingUser is null)
                {
                    User NewUser= User.CreateUserAndLinkToExternalProvider
                    (
                        command.Email,
                        command.FullName,
                        command.Provider,
                        command.ProviderUserId,
                        _clock.UtcNow,
                        command.ProviderEmail
                    );
                    // Despues de crear al usuario, debo crear tambien un dispositivo y agregarselo
                    DeviceToken device= NewUser.RegisterDevice
                    (
                        command.DeviceIdentifier,
                        command.DeviceName,
                        command.Platform,
                        _clock.UtcNow
                    );
                    // Despues tengo que emitir un refreshtoken
                    RefreshToken refreshtoken=device.IssueRefreshToken(command.Ip, _clock.UtcNow);
                    await _userRepository.AddAsync(NewUser,ct);

                    // Genero un access token
                    string jsonWebToken= _jwtTokenGenerator.GenerateToken(NewUser);
                    await _uow.SaveChangesAsync(ct);
                    return new GoogleLoginResult(jsonWebToken,refreshtoken.Token,true);
                }
                // En caso de que el usuario tenga cuenta (email y contraseña) pero es la primera vez que se loggea con google
                else
                {
                    // Unimos al usuario existente con google
                    existingUser.LinkUserWithExternalProvider
                    (
                        command.Provider,
                        command.ProviderUserId,
                        _clock.UtcNow,
                        command.ProviderEmail
                    );
                    // Creamos un dispositivo
                    DeviceToken device= existingUser.RegisterDevice   
                    (
                        command.DeviceIdentifier,
                        command.DeviceName,
                        command.Platform,
                        _clock.UtcNow
                    );
                    // emito refresh token
                    RefreshToken refreshToken=device.IssueRefreshToken(command.Ip,_clock.UtcNow);
                    await _uow.SaveChangesAsync(ct);              
                    // emito access token
                    string jsonWebToken=_jwtTokenGenerator.GenerateToken(existingUser);
                    return new GoogleLoginResult(jsonWebToken,refreshToken.Token,false);

                }

            }
            // En caso de que exista el usuario y ya este vinculado con el proveedor
            else
            {
                // Registro el dispositivo
                    DeviceToken device= existingUser.RegisterDevice   
                    (
                        command.DeviceIdentifier,
                        command.DeviceName,
                        command.Platform,
                        _clock.UtcNow
                    );
                    // emito refresh token
                    RefreshToken refreshToken=device.IssueRefreshToken(command.Ip,_clock.UtcNow);
                    // emito access token
                    string jsonWebToken=_jwtTokenGenerator.GenerateToken(existingUser);
                    return new GoogleLoginResult(jsonWebToken,refreshToken.Token,false);
            }
        }catch(Exception e)
        {
            throw;
        }
        
    }
}
