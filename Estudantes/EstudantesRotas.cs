using ApiCrud.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCrud.Estudantes
{
    public static class EstudantesRotas
    {

        public static void AddRotasEstudantes(this WebApplication app)
        {
            var rotasEstudantes = app.MapGroup(prefix: "estudantes");

            // para criar se usa o post 
            rotasEstudantes.MapPost(pattern: "", handler: async (AddEstudanteRequest request, AppDbContext context, CancellationToken cancellationToken) =>
            {
                var jaExiste = await context.Estudantes.AnyAsync(Estudante => Estudante.Nome == request.Nome);

                if (jaExiste)
                    return Results.Conflict("Ja existe!");

                var novoEstudante = new Estudante(request.Nome);

                await context.Estudantes.AddAsync(novoEstudante,cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                var estudanteRetorno = new EstudanteDto(novoEstudante.Id, novoEstudante.Nome);

                return Results.Ok(estudanteRetorno);
            });
             
             // Retorna Todos os estudantes cadastrados 

            rotasEstudantes.MapGet(pattern: "", handler: async (AppDbContext context, CancellationToken cancellationToken) =>

            {
                var estudantes = await context
                .Estudantes
                .Where(estudante => estudante.Ativo == true)
                .Select(Estudante => new EstudanteDto(Estudante.Id, Estudante.Nome))
                .ToListAsync(cancellationToken);

                return estudantes;
            });


            // Atuaizar nome dos estudantes 

            rotasEstudantes.MapPut(pattern: "{Id:guid}",
                handler: async ( Guid id , UpdateEstudanteRequest request, AppDbContext context, CancellationToken cancellationToken) =>
            {
                var estudante = await context.Estudantes.SingleOrDefaultAsync(estudante => estudante.Id == id, cancellationToken);

                if (estudante == null)
                    return Results.NotFound();

                estudante.AtualizarNome(request.Nome);

                await context.SaveChangesAsync(cancellationToken);
                return Results.Ok(new EstudanteDto(estudante.Id,estudante.Nome));

            });

            // Deletar

            rotasEstudantes.MapDelete(pattern: "{id}",
                handler: async (Guid id, AppDbContext context, CancellationToken cancellationToken) =>
                {
                    var estudante = await context.Estudantes.SingleOrDefaultAsync(estudante => estudante.Id == id, cancellationToken);

                    if (estudante == null)
                        return Results.NotFound();


                    estudante.Desativar();

                    await context.SaveChangesAsync(cancellationToken);
                    return Results.Ok();
                });


            }
        }


}

