using FootballCatch.Auth.Domain.Users.Repository;
using FootballCatch.Auth.Domain.Users.Services;
using FootballCatch.Common.BuildingBlocks.Persistance;
using FootballCatch.Common.Mediator;
using FootballCatch.Common.Types;

namespace FootballCatch.Auth.Application.Users.Commands.OtpLogin.SendOtpCode;

public sealed class SendOtpCodeCommandHandler : ICommandHandler<SendOtpCodeCommand>
{
    private readonly IEmailSender _emailSender;
    private readonly IHashService _hashService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public SendOtpCodeCommandHandler
    (
        IEmailSender emailSender, 
        IHashService hashService, 
        IUserRepository userRepository, 
        IUnitOfWork uow,
        IClock clock
    )
    {
        _emailSender = emailSender;
        _hashService = hashService;
        _userRepository = userRepository;
        _uow = uow;
        _clock = clock;
    }

    public async Task HandleAsync(SendOtpCodeCommand command, CancellationToken ct = default)
    {
        string Code = GenerateOtpCode();
        OtpCode otpCode =  OtpCode.Create(Code,_hashService,_clock.UtcNow);
        await _userRepository.AddOtpCodeAsync(otpCode.Hash);
        await _emailSender.Send(command.Email,"Verification code",Code);
        await _uow.SaveChangesAsync(ct);
    }

    public string GenerateOtpCode(int length = 6)
    {
        return Random.Shared.Next(0, (int)Math.Pow(10, length))
                    .ToString()
                    .PadLeft(length, '0');
    }
}