namespace ConectaFranquias.Api.Excecoes;

public class ExcecaoNegocio(string mensagem) : Exception(mensagem);

public class ExcecaoNaoEncontrado : Exception
{
    public ExcecaoNaoEncontrado(string mensagem) : base(mensagem)
    {
    }

    public ExcecaoNaoEncontrado(string recurso, object identificador)
        : base($"{recurso} de identificador '{identificador}' não foi encontrado(a).")
    {
    }
}

public class ExcecaoConflito(string mensagem) : Exception(mensagem);

public class ExcecaoAutorizacao(string mensagem) : Exception(mensagem);
