# Changelog

## 1.0.1

- Corrige o carregamento do patch de `InventoryGrid.OnLeftDown` no Valheim 1.0.7.
- Harmony agora identifica os argumentos pela posicao, sem depender dos nomes internos.

## 1.0.0

- Alt + clique esquerdo alterna favorito no inventario do jogador.
- Estrela dourada discreta no slot favorito.
- Persistencia via `ItemData.m_customData` sem impedir o empilhamento vanilla.
- Propagacao segura do favorito em divisao e mesclagem de stacks.
- Protecao do quick stack do GorilaChestMod 2.2.2.
- Protecao automatica opcional dos slots do EquipmentAndQuickSlots 3.x.
