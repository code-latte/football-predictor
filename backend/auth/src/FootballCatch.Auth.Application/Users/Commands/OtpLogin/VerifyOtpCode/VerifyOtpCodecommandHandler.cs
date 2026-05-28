using FootballCatch.Auth.Domain.Users.Aggregates;
using FootballCatch.Auth.Domain.Users.Repository;
using FootballCatch.Auth.Domain.Users.Services;
using FootballCatch.Common.BuildingBlocks.Persistance;
using FootballCatch.Common.Mediator;
using FootballCatch.Common.Types;

namespace FootballCatch.Auth.Application.Users.Commands.OtpLogin.VerifyOtpCode;

public sealed class VerifyOtpCodeCommandHandler : ICommandHandler<VerifyOtpCodeCommand, OtpLoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IHashService _hashService;
    private readonly IClock _clock;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _uow;

    public VerifyOtpCodeCommandHandler(IUserRepository userRepository, IHashService hashService, IClock clock, IJwtTokenGenerator jwtTokenGenerator, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _hashService = hashService;
        _clock = clock;
        _jwtTokenGenerator = jwtTokenGenerator;
        _uow = uow;
    }

    public async Task<OtpLoginResult> HandleAsync(VerifyOtpCodeCommand command, CancellationToken ct = default)
    {
        OtpCode code = await _userRepository.GetHashedCodeAsync(command.Email);
        // Si el codigo es incorrecto lanzo una excepcion de dominio
        code.Validate(command.Code,_hashService,_clock.UtcNow);
        User? existingUser = await _userRepository.FindByEmailAsync(command.Email,ct);
        // Si el usuario existe registro el device e inicio sesion
        if(existingUser is not null)
        {
            DeviceToken device = existingUser.RegisterDevice
            (
                command.DeviceIdentifier,
                command.DeviceName,
                command.Platform,
                _clock.UtcNow
            );

        RefreshToken refreshToken=device.IssueRefreshToken(command.Ip,_clock.UtcNow);
        await _uow.SaveChangesAsync(ct);              
        // emito access token
        string jsonWebToken=_jwtTokenGenerator.GenerateToken(existingUser);
        return new OtpLoginResult(jsonWebToken,refreshToken.Token,false);
        }
        // si no existe, el front debe mandar al usuario a un formulario de onboarding
        return new OtpLoginResult(string.Empty, string.Empty, true);

        
    }
}