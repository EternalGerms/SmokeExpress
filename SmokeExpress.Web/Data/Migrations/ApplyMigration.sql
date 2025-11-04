-- Migration: AddUserFieldsAndRemoveEnderecoFields
-- Adiciona campos necessários e remove campos de endereço detalhado da tabela AspNetUsers

-- Primeiro, verificar se as colunas já existem antes de adicionar
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'DocumentoFiscal')
BEGIN
    ALTER TABLE AspNetUsers ADD DocumentoFiscal nvarchar(14) NOT NULL DEFAULT '';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'TipoCliente')
BEGIN
    ALTER TABLE AspNetUsers ADD TipoCliente nvarchar(2) NOT NULL DEFAULT 'PF';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Genero')
BEGIN
    ALTER TABLE AspNetUsers ADD Genero nvarchar(30) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'ConsentiuMarketing')
BEGIN
    ALTER TABLE AspNetUsers ADD ConsentiuMarketing bit NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'TermosAceitosEm')
BEGIN
    ALTER TABLE AspNetUsers ADD TermosAceitosEm datetime2 NULL;
END

-- Tornar Endereco nullable (se ainda não estiver)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Endereco' AND is_nullable = 0)
BEGIN
    ALTER TABLE AspNetUsers ALTER COLUMN Endereco nvarchar(500) NULL;
END

-- Remover colunas de endereço detalhado (se existirem)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Cep')
BEGIN
    ALTER TABLE AspNetUsers DROP COLUMN Cep;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Logradouro')
BEGIN
    ALTER TABLE AspNetUsers DROP COLUMN Logradouro;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Numero')
BEGIN
    ALTER TABLE AspNetUsers DROP COLUMN Numero;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Complemento')
BEGIN
    ALTER TABLE AspNetUsers DROP COLUMN Complemento;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Bairro')
BEGIN
    ALTER TABLE AspNetUsers DROP COLUMN Bairro;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Cidade')
BEGIN
    ALTER TABLE AspNetUsers DROP COLUMN Cidade;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'Estado')
BEGIN
    ALTER TABLE AspNetUsers DROP COLUMN Estado;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'PontoReferencia')
BEGIN
    ALTER TABLE AspNetUsers DROP COLUMN PontoReferencia;
END

-- Criar índice único em DocumentoFiscal (se não existir)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'IX_AspNetUsers_DocumentoFiscal')
BEGIN
    CREATE UNIQUE INDEX IX_AspNetUsers_DocumentoFiscal ON AspNetUsers(DocumentoFiscal);
END

