using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogiMarket.Application.Commands;
public interface ICommandInvoker
{
    Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
    Task<IReadOnlyList<RegistroComando>> HistorialAsync(CancellationToken ct = default);
}