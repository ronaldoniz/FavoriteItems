# FavoriteItems para Valheim 1.0

Mod client-side simples para marcar stacks do inventario como favoritos e impedir que o
**GorilaChestMod** os envie para baus durante o quick stack.

## Uso

1. Abra o inventario.
2. Segure `Alt` esquerdo ou direito.
3. Clique com o botao esquerdo em um stack.
4. Uma estrela dourada discreta indica que o stack esta protegido.

Repita `Alt + clique` para desfavoritar.

## Compatibilidade implementada

- **GorilaChestMod 2.2.2**: aplica um postfix Harmony em
  `GorilaChestMod.QuickStack.CanMove(Player, Inventory, ItemData)`. Itens favoritos recebem
  `false` antes da movimentacao.
- **EquipmentAndQuickSlots 3.x**: integracao opcional por reflexao com a API publica
  `EquipmentAndQuickSlots.API.IsSlotCell`. Todos os slots especiais ativos ficam protegidos
  automaticamente, inclusive quick slots, mesmo sem estrela.
- Os dois mods sao dependencias opcionais. FavoriteItems continua carregando sem eles.

## Persistencia e stacks

O favorito e salvo somente nesta chave do `ItemData.m_customData`:

```
com.ronaldo.valheim.favoriteitems.favorite = 1
```

Isso preserva todos os dados de outros mods. No Valheim 1.0.7, `m_customData` e salvo com o
personagem e copiado por `ItemData.Clone()`, mas nao participa de `IsSameType` nem de
`FindFreeStackItem`; por isso a estrela nao impede stacks iguais de se juntarem.

Quando um stack favorito e dividido, os dois lados continuam favoritos. Quando ele e mesclado
em outro stack compativel, o stack de destino tambem passa a ser favorito. Ao desfavoritar,
somente a chave acima e removida.

## Instalacao

Requisitos:

- Valheim 1.0 com backend Mono;
- BepInEx 5 (`denikson-BepInExPack_Valheim`);
- GorilaChestMod apenas se quiser a protecao no quick stack;
- EquipmentAndQuickSlots apenas se quiser os slots extras.

### Thunderstore Mod Manager / r2modman

Copie `FavoriteItems.dll` para:

```
<perfil>\BepInEx\plugins\ronaldoniz-FavoriteItems\FavoriteItems.dll
```

### Instalacao manual

Copie `FavoriteItems.dll` para:

```
<Valheim>\BepInEx\plugins\ronaldoniz-FavoriteItems\FavoriteItems.dll
```

Inicie o jogo e confirme em `BepInEx\LogOutput.log`:

```
FavoriteItems 1.0.1 carregado
Protecao do quick stack do GorilaChestMod ativa
```

O arquivo de configuracao e criado em:

```
BepInEx\config\com.ronaldo.valheim.favoriteitems.cfg
```

## Build

Instale o .NET SDK e execute na pasta do projeto:

```powershell
dotnet build -c Release
```

Se o Valheim estiver em outra pasta:

```powershell
dotnet build -c Release -p:ValheimDir="D:\SteamLibrary\steamapps\common\Valheim"
```

A DLL sera criada em `bin\Release\FavoriteItems.dll`.

O projeto nao inclui as DLLs do Valheim. O build usa os assemblies da instalacao local do
jogo indicada por `ValheimDir`.

## Estado dos testes

- Carregamento e uso em partida solo: aprovado pelo autor.
- Carregamento e uso em servidor multiplayer: aprovado pelo autor.
- Build local da versao publicada: verificado antes da Release.

Esses testes foram realizados pelo autor no ambiente de jogo dele; nao constituem uma
certificacao independente de compatibilidade com todas as combinacoes de mods.

## Releases e atualizacoes

As versoes compiladas ficam em
[GitHub Releases](https://github.com/ronaldoniz/FavoriteItems/releases). Cada correcao recebe
uma nova versao seguindo versionamento semantico; os arquivos de releases anteriores nao sao
substituidos.

## Verificacao recomendada dentro do jogo

1. Crie um perfil de teste com BepInEx, GorilaChestMod e FavoriteItems.
2. Coloque dois stacks iguais no inventario e o mesmo item em um bau proximo.
3. Favorite apenas um stack e use o quick stack do Gorila.
4. Confirme que o favorito ficou e o outro stack foi movido.
5. Divida e depois una o stack favorito; confirme a estrela nos stacks resultantes.
6. Com EquipmentAndQuickSlots, coloque comida em `Quick1` e confirme que ela nao e movida.
7. Saia normalmente, entre de novo e confirme que a estrela persistiu.

## Limites desta versao

- O gesto e voltado a teclado e mouse; nao ha atalho de gamepad.
- A integracao EAQS requer a API `IsSlotCell` das versoes 3.x atuais. Se a API mudar, o log
  avisa e apenas essa protecao automatica e desativada; favoritos continuam funcionando.
- A protecao implementada e especifica ao quick stack do GorilaChestMod.
