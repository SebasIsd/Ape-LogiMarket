namespace LogiMarket.WinForms.Interpreter;
public static class InterpreterFiltro
{
    public static ExpresionFiltro Parse(string expresion)
    {
        if (string.IsNullOrWhiteSpace(expresion))
            throw new ArgumentException("Expresión vacía");

        // Tokenización simple
        var tokens = Tokenizar(expresion);
        int pos = 0;
        var resultado = ParseOr(tokens, ref pos);
        if (pos < tokens.Count)
            throw new ArgumentException($"Sintaxis inválida en: {tokens[pos]}");
        return resultado;
    }

    private static List<string> Tokenizar(string exp)
    {
        var tokens = new List<string>();
        var actual = "";
        foreach (var c in exp)
        {
            if (char.IsWhiteSpace(c))
            {
                if (actual.Length > 0) { tokens.Add(actual); actual = ""; }
            }
            else if (c == '(' || c == ')')
            {
                if (actual.Length > 0) { tokens.Add(actual); actual = ""; }
                tokens.Add(c.ToString());
            }
            else
            {
                actual += c;
            }
        }
        if (actual.Length > 0) tokens.Add(actual);
        return tokens;
    }

    private static ExpresionFiltro ParseOr(List<string> tokens, ref int pos)
    {
        var izq = ParseAnd(tokens, ref pos);
        while (pos < tokens.Count && tokens[pos].ToUpper() == "OR")
        {
            pos++;
            var der = ParseAnd(tokens, ref pos);
            izq = new ExpresionOr(izq, der);
        }
        return izq;
    }

    private static ExpresionFiltro ParseAnd(List<string> tokens, ref int pos)
    {
        var izq = ParsePrimario(tokens, ref pos);
        while (pos < tokens.Count && tokens[pos].ToUpper() == "AND")
        {
            pos++;
            var der = ParsePrimario(tokens, ref pos);
            izq = new ExpresionAnd(izq, der);
        }
        return izq;
    }

    private static ExpresionFiltro ParsePrimario(List<string> tokens, ref int pos)
    {
        if (pos >= tokens.Count)
            throw new ArgumentException("Expresión incompleta");

        if (tokens[pos] == "(")
        {
            pos++;
            var expr = ParseOr(tokens, ref pos);
            if (pos >= tokens.Count || tokens[pos] != ")")
                throw new ArgumentException("Falta paréntesis de cierre");
            pos++;
            return expr;
        }

        // campo operador valor
        if (pos + 2 >= tokens.Count)
            throw new ArgumentException($"Sintaxis inválida: {tokens[pos]}");

        var campo = tokens[pos];
        var op = tokens[pos + 1];
        var valor = tokens[pos + 2];
        pos += 3;

        if (op == "=")
            return new ExpresionIgualdad(campo, valor);
        if (op == ">=")
        {
            if (!decimal.TryParse(valor, out var num))
                throw new ArgumentException($"Valor numérico esperado: {valor}");
            return new ExpresionMayorIgual(campo, num);
        }

        throw new ArgumentException($"Operador no soportado: {op}");
    }
}