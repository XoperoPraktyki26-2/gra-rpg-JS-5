using BibliotekaRPG.map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BibliotekaRPG;
using BibliotekaRPG.Inventory;
using BibliotekaRPG.Quests;
using BibliotekaRPG.Npcs;

namespace WpfRpg
{
    public partial class MainWindow : Window, IGameEventListener
    {
        private MapSession _session = null!;
        private CancellationTokenSource? _moveCts;
        private string? _currentNpcName;

        public MainWindow()
        {
            InitializeComponent();
            InitGame();
        }

        private void InitGame()
        {
            _session = new MapSession();
            _session.RegisterListener(this);
            _session.Log += Log;
            _session.MapChanged += RenderMap;
            _session.BattleStarted += _ => UpdateEnemyUI();
            _session.EnemyRewardsProcessed += _ =>
            {
                UpdateEnemyUI();
                UpdateQuestUI();
            };
            _session.NpcEncountered += OnNpcEncountered;

            playerName.Content = _session.Player.Name;

            UpdatePlayerUI();
            UpdateExpUI();
            RefreshItemsDropdown();
            RefreshEquipmentLists();
            updateStatsPanel();
            RenderMap();
            UpdateQuestUI();
        }

        private async void Tile_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is ValueTuple<int, int> coords)
            {
                _moveCts?.Cancel();
                _moveCts = new CancellationTokenSource();

                var (r, c) = coords;

                try
                {
                    await MoveTo(r, c, _moveCts.Token);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    Log($"Błąd ruchu: {ex.Message}");
                }
            }
        }

        private async Task MoveTo(int targetRow, int targetCol, CancellationToken token)
        {
            var start = _session.PlayerPosition;
            var path = _session.PathFinder.FindPath(start, (targetRow, targetCol));

            if (path == null)
            {
                Log("Nie znaleziono drogi.");
                return;
            }

            foreach (var step in path)
            {
                if (token.IsCancellationRequested) return;

                if (_session.HasActiveBattle)
                {
                    Log("Zatrzymano: walka!");
                    break;
                }

                int dr = step.Row - _session.PlayerPosition.Row;
                int dc = step.Col - _session.PlayerPosition.Col;

                if (!_session.TryMove(dr, dc))
                    break;

                try
                {
                    await Task.Delay(200, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                if (_session.HasActiveBattle)
                    break;
            }
        }

        private void RenderMap()
        {
            if (_session?.Map == null)
                return;

            UpdateActionButtonState();
            UpdateMerchantShopVisibility();

            var map = _session.Map;
            mapGrid.Rows = map.Size;
            mapGrid.Columns = map.Size;
            mapGrid.Children.Clear();

            for (int row = 0; row < map.Size; row++)
            {
                for (int col = 0; col < map.Size; col++)
                {
                    var tile = map.GetTile(row, col);
                    if (tile == null)
                        continue;

                    var border = new Border
                    {
                        Background = GetTileBrush(tile, row, col),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5),
                        Child = new TextBlock
                        {
                            Text = GetTileLabel(tile),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.White
                        }
                    };

                    border.Tag = (row, col);
                    border.PreviewMouseLeftButtonDown += Tile_Click;
                    border.ToolTip = tile.Type.ToString();

                    mapGrid.Children.Add(border);
                }
            }
        }

        private Brush GetTileBrush(ITile tile, int row, int col)
        {
            if (_session != null &&
                _session.PlayerPosition.Row == row &&
                _session.PlayerPosition.Col == col)
                return Brushes.Gold;

            return tile.Type switch
            {
                ITile.TileType.Grass => Brushes.LightGreen,
                ITile.TileType.Forest => Brushes.Green,
                ITile.TileType.Mountain => Brushes.SlateGray,
                ITile.TileType.EnemySpawn => Brushes.DarkRed,
                ITile.TileType.Treasure => Brushes.DarkGoldenrod,
                ITile.TileType.Empty => Brushes.SandyBrown,
                ITile.TileType.Merchant => Brushes.MediumPurple,
                _ => Brushes.LightGray
            };
        }

        private string GetTileLabel(ITile tile)
        {
            return tile.Type switch
            {
                ITile.TileType.Grass => "G",
                ITile.TileType.Forest => "F",
                ITile.TileType.Mountain => "M",
                ITile.TileType.EnemySpawn => "E",
                ITile.TileType.Treasure => "T",
                ITile.TileType.Empty => ".",
                ITile.TileType.Merchant => "K",
                _ => ""
            };
        }

        private void TryMovePlayer(int deltaRow, int deltaCol)
        {
            _moveCts?.Cancel();
            _session?.TryMove(deltaRow, deltaCol);
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e) => TryMovePlayer(-1, 0);
        private void MoveDown_Click(object sender, RoutedEventArgs e) => TryMovePlayer(1, 0);
        private void MoveLeft_Click(object sender, RoutedEventArgs e) => TryMovePlayer(0, -1);
        private void MoveRight_Click(object sender, RoutedEventArgs e) => TryMovePlayer(0, 1);

        private void UpdatePlayerUI()
        {
            var player = _session.Player;
            playerLevel.Content = $"Lvl {player.Level}";
            HealthBar.Maximum = player.MaxHealth;
            HealthBar.Value = player.Health;
            playerGoldLabel.Content = $"Złoto: {player.Gold}";
        }

        private void UpdateEnemyUI()
        {
            var enemy = _session.CurrentEnemy;
            if (enemy == null)
            {
                enemyHealthBar.Maximum = 1;
                enemyHealthBar.Value = 0;
                enemyName.Content = "Brak przeciwnika";
                enemyLevel.Content = "Lvl -";
                return;
            }

            enemyName.Content = enemy.Name;
            enemyLevel.Content = $"Lvl {enemy.Level}";
            enemyHealthBar.Maximum = enemy.MaxHealth;
            enemyHealthBar.Value = enemy.Health;
        }

        private void UpdateExpUI()
        {
            var player = _session.Player;
            levelBar.Maximum = player.ExperienceToNextLevel;
            levelBar.Value = player.Experience;
        }

        private void RefreshItemsDropdown()
        {
            itemDropdown.Items.Clear();

            foreach (var item in _session.Player.Inventory)
                itemDropdown.Items.Add(item.Name);

            if (itemDropdown.Items.Count > 0)
                itemDropdown.SelectedIndex = 0;
        }

        private void RefreshMerchantOffersList()
        {
            merchantOffersList.Items.Clear();

            var merchant = _session.GetMerchantAtPlayerPosition();
            if (merchant == null)
                return;

            foreach (var offer in merchant.Offers)
                merchantOffersList.Items.Add(new MerchantOfferListEntry(offer));
        }

        private void UpdateActionButtonState()
        {
            var onMerchant = _session.GetMerchantAtPlayerPosition() != null;
            attackBtn.Content = onMerchant ? "Sklep" : "Atak";
        }

        private void UpdateMerchantShopVisibility()
        {
            if (_session.GetMerchantAtPlayerPosition() == null)
            {
                merchantShopGrid.Visibility = Visibility.Collapsed;
                return;
            }

            if (merchantShopGrid.Visibility == Visibility.Visible)
                RefreshMerchantOffersList();
        }

        private void UpdateQuestUI()
        {
            questList.Items.Clear();
            foreach (var quest in _session.ActiveQuests)
            {
                questList.Items.Add(new QuestListEntry(quest));
            }
        }

        private void RefreshEquipmentLists()
        {
            inventoryEquipList.Items.Clear();
            equippedList.Items.Clear();

            for (int i = 0; i < _session.Player.Inventory.Count; i++)
            {
                if (_session.Player.Inventory[i] is EquipmentItem eq)
                    inventoryEquipList.Items.Add(new EquipmentListEntry(eq));
            }

            foreach (var eq in _session.Player.EquippedWeapons)
                equippedList.Items.Add(new EquipmentListEntry(eq));

            foreach (var eq in _session.Player.EquippedArmors)
                equippedList.Items.Add(new EquipmentListEntry(eq));
        }

        private void Log(string msg)
        {
            logList.Items.Add(msg);
            logList.ScrollIntoView(logList.Items[logList.Items.Count - 1]);
        }

        private void useItemBtn_Click(object sender, RoutedEventArgs e)
        {
            if (itemDropdown.SelectedIndex >= 0)
            {
                _session.Player.UseItem(itemDropdown.SelectedIndex);
                RefreshItemsDropdown();
                RefreshEquipmentLists();
                UpdatePlayerUI();
            }
        }

        public void OnAttack(Character attacker, Character target, int damage)
        {
            Log($"{attacker.Name} atakuje {target.Name} za {damage}. HP celu: {target.Health}");
            if (attacker == _session.Player)
                UpdateEnemyUI();
            else
                UpdatePlayerUI();
        }

        public void OnLevelUp(Player player)
        {
            Log($"LEVEL UP! {player.Name} osiągnął poziom {player.Level}!");
            UpdatePlayerUI();
            UpdateExpUI();
        }

        public void OnBattleStart(Enemy enemy)
        {
            Log($"Walka z {enemy.Name}!");
            UpdateEnemyUI();
        }

        public void OnEnemyDefeated(Enemy enemy)
        {
            Log($"Pokonałeś {enemy.Name}!");
            RefreshItemsDropdown();
            RefreshEquipmentLists();
            UpdatePlayerUI();
            UpdateExpUI();
            updateStatsPanel();
            UpdateEnemyUI();
        }

        public void OnPlayerDefeated(Player player)
        {
            Log($"{player.Name} poległ. Git Gut.");
            Close();
        }

        public void OnItemUsed(Character user, IItem item)
        {
            Log($"{user.Name} używa {item.Name}");
            RefreshEquipmentLists();
            UpdatePlayerUI();
        }

        public void OnEquipmentPutOn(IStatModifier item)
        {
            Log("Założono: " + item.Name);
            updateStatsPanel();
        }

        public void OnShowStats(Player player)
        {
            Log($"HP: {player.Health}/{player.MaxHealth} | Atak: {player.AttackPower} | Level: {player.Level}");
        }

        public void OnShowInventory(List<IItem> items)
        {
            Log("Przedmioty: " + (items.Count == 0 ? "brak" : string.Join(", ", items.ConvertAll(i => i.Name))));
        }

        public void OnShowEquipment(IStatModifier[] items)
        {
            var names = new List<string>();
            foreach (var m in items)
                if (m != null) names.Add(m.Name);

            Log("Założone: " + (names.Count == 0 ? "brak" : string.Join(", ", names)));
        }

        public void OnEquipmentSlotsFull()
        {
            Log("Nie masz już wolnych slotów na ekwipunek.");
        }

        private void updateStatsPanel()
        {
            var player = _session.Player;
            hpStatLabel.Content = $"Hp: {player.Health}/{player.MaxHealth}";
            attackStatLabel.Content = $"Power: {player.AttackPower}";
            levelStatLabel.Content = $"Level: {player.Level} | XP: {player.Experience}/{player.ExperienceToNextLevel}";

            modifiersList.Items.Clear();
            foreach (var m in _session.Equipment.modifiers)
            {
                if (m != null)
                    modifiersList.Items.Add(m.Name);
            }
        }

        private void equipmentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (equipmentGrid.Visibility == Visibility.Visible)
            {
                equipmentGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                RefreshEquipmentLists();
                equipmentGrid.Visibility = Visibility.Visible;
            }
        }

        private void inventoryEquipList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (inventoryEquipList.SelectedItem is not EquipmentListEntry entry)
                return;

            _session.Player.Equip(entry.Item);
            Log("Założono: " + entry.Item.Name);
            RefreshItemsDropdown();
            RefreshEquipmentLists();
            updateStatsPanel();
            UpdatePlayerUI();
        }

        private void equippedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (equippedList.SelectedItem is not EquipmentListEntry entry)
                return;

            if (_session.Player.Unequip(entry.Item))
            {
                Log("Zdjęto: " + entry.Item.Name);
                RefreshItemsDropdown();
                RefreshEquipmentLists();
                updateStatsPanel();
                UpdatePlayerUI();
            }
        }

        private void attackBtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (_session.GetMerchantAtPlayerPosition() != null)
            {
                ToggleMerchantShop();
                return;
            }

            if (_session.CurrentEnemy == null || !_session.CurrentEnemy.IsAlive())
                _session.SpawnEnemy();

            var enemy = _session.CurrentEnemy;
            if (enemy == null)
                return;

            if (radioBtnMele.IsChecked == true)
                _session.Player.ChooseAttack(new MeleeAttack());
            else
                _session.Player.ChooseAttack(new MagicAttack());

            _session.Player.PerformAttack(enemy);
            UpdateEnemyUI();

            if (!enemy.IsAlive())
            {
                _session.ResolveEnemyDefeat(enemy);
                UpdatePlayerUI();
                UpdateExpUI();
                return;
            }

            enemy.PerformAttack(_session.Player);
            UpdatePlayerUI();

            if (!_session.Player.IsAlive())
            {
                OnPlayerDefeated(_session.Player);
                attackBtn.IsEnabled = false;
            }
        }

        private void ToggleMerchantShop()
        {
            if (_session.GetMerchantAtPlayerPosition() == null)
            {
                merchantShopGrid.Visibility = Visibility.Collapsed;
                return;
            }

            if (merchantShopGrid.Visibility == Visibility.Visible)
            {
                merchantShopGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                RefreshMerchantOffersList();
                merchantShopGrid.Visibility = Visibility.Visible;
            }
        }

        private void buyOfferBtn_Click(object sender, RoutedEventArgs e)
        {
            BuySelectedOffer();
        }

        private void merchantOffersList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BuySelectedOffer();
        }

        private void BuySelectedOffer()
        {
            if (merchantOffersList.SelectedItem is not MerchantOfferListEntry entry)
                return;

            var merchant = _session.GetMerchantAtPlayerPosition();
            if (merchant == null)
            {
                merchantShopGrid.Visibility = Visibility.Collapsed;
                return;
            }

            var offerIndex = merchant.Offers.IndexOf(entry.Offer);
            if (offerIndex < 0)
            {
                RefreshMerchantOffersList();
                return;
            }

            if (_session.BuyFromMerchant(offerIndex, out var message))
            {
                Log(message);
                RefreshItemsDropdown();
                RefreshEquipmentLists();
                updateStatsPanel();
                UpdatePlayerUI();
                RefreshMerchantOffersList();

                if (_session.GetMerchantAtPlayerPosition() == null)
                {
                    merchantShopGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                Log(message);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool handled = true;
            switch (e.Key)
            {
                case Key.W:
                case Key.Up:
                    TryMovePlayer(-1, 0);
                    break;
                case Key.S:
                case Key.Down:
                    TryMovePlayer(1, 0);
                    break;
                case Key.A:
                case Key.Left:
                    TryMovePlayer(0, -1);
                    break;
                case Key.D:
                case Key.Right:
                    TryMovePlayer(0, 1);
                    break;
                default:
                    handled = false;
                    break;
            }

            if (handled)
                e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }

        private void statsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (statGrid.Visibility == Visibility.Visible)
            {
                statGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                updateStatsPanel();
                statGrid.Visibility = Visibility.Visible;
            }
        }

        private void SaveGame_Click(object sender, RoutedEventArgs e)
        {
            var saver = new Saver();
            var snapshot = _session.CreateSnapshot();
            saver.Save(snapshot);
            Log("Gra zapisana.");
        }

        private void LoadGame_Click(object sender, RoutedEventArgs e)
        {
            var saver = new Saver();
            var loaded = saver.Load();

            if (loaded == null)
            {
                Log("Brak zapisu gry.");
                return;
            }

            _session.LoadSnapshot(loaded);
            Log("Wczytano zapis gry.");

            UpdatePlayerUI();
            UpdateEnemyUI();
            UpdateExpUI();
            RefreshItemsDropdown();
            RefreshEquipmentLists();
            UpdateActionButtonState();
            UpdateMerchantShopVisibility();
            updateStatsPanel();
            RenderMap();
            UpdateQuestUI();
        }

        private void UndoTurn_Click(object sender, RoutedEventArgs e)
        {
            _moveCts?.Cancel();

            if (_session.UndoTurn())
            {
                Log("Cofnięto turę.");
                UpdatePlayerUI();
                UpdateEnemyUI();
                UpdateExpUI();
                RefreshItemsDropdown();
                RefreshEquipmentLists();
                UpdateActionButtonState();
                UpdateMerchantShopVisibility();
                updateStatsPanel();
                RenderMap();
                UpdateQuestUI();
            }
            else
            {
                Log("Nie udało się cofnąć tury.");
            }
        }

        private void OpenDialog_Click(object sender, RoutedEventArgs e)
        {
            ShowDialogPanel("Strażnik polany", "Strażnik patrolu", "SP", BuildDialogIntro(), new[]
            {
                new DialogOption("Opowiedz o zadaniach", DescribeQuests),
                new DialogOption("Szczegóły patroli", () => dialogContent.Text = BuildQuestDetailText()),
                new DialogOption("Podsumowanie", () => dialogContent.Text = BuildQuestSummary()),
                new DialogOption("Zamknij", CloseDialog)
            });
        }

        private void OnNpcEncountered(NpcEncounterInfo info)
        {
            Dispatcher.Invoke(() =>
            {
                _currentNpcName = info.Npc.Name;
                ShowDialogPanel(
                    info.Npc.Name,
                    info.Npc.Role,
                    GetAvatarInitial(info.Npc.Name),
                    BuildNpcIntro(info.Npc),
                    new[]
                    {
                        new DialogOption("Poproś o zadanie", () => dialogContent.Text = BuildNpcTaskText(info.Npc)),
                        new DialogOption("Pokaż szczegóły", () => dialogContent.Text = BuildNpcIntro(info.Npc)),
                        new DialogOption("Zakończ", CloseDialog),
                        new DialogOption("Zaatakuj", TriggerNpcCombatFromDialog, true)
                    });
            });
        }

        private void ShowDialogPanel(string title, string role, string avatar, string content, IEnumerable<DialogOption> options)
        {
            dialogNpcName.Text = title;
            dialogNpcRole.Text = role;
            dialogNpcAvatar.Text = avatar;
            dialogContent.Text = content;
            SetDialogButtons(options);
            dialogOverlay.Visibility = Visibility.Visible;
        }

        private void SetDialogButtons(IEnumerable<DialogOption> options)
        {
            dialogButtonsPanel.Children.Clear();

            foreach (var option in options)
            {
                var textBlock = new TextBlock
                {
                    Text = $"> {option.Label}",
                    Cursor = Cursors.Hand,
                    FontFamily = new FontFamily("Courier New"),
                    FontSize = 20,
                    Margin = new Thickness(0, 6, 0, 0),
                    Foreground = option.LeadsToCombat ? Brushes.OrangeRed : Brushes.LightCyan
                };

                textBlock.MouseLeftButtonUp += (_, _) => option.Action?.Invoke();
                textBlock.MouseEnter += (_, _) => textBlock.TextDecorations = TextDecorations.Underline;
                textBlock.MouseLeave += (_, _) => textBlock.TextDecorations = null;

                dialogButtonsPanel.Children.Add(textBlock);
            }
        }

        private void DescribeQuests()
        {
            dialogContent.Text = BuildQuestDetailText();
        }

        private string BuildNpcIntro(NpcData npc)
        {
            var sb = new StringBuilder();
            sb.AppendLine(npc.Dialogue);
            sb.AppendLine();
            sb.AppendLine($"Wzajemna opinia: {npc.Opinion}%");
            sb.AppendLine($"Wskazówka misji: {npc.TaskHint}");
            return sb.ToString().TrimEnd();
        }

        private string BuildNpcTaskText(NpcData npc)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Zadanie:");
            sb.AppendLine($"• {npc.TaskHint}");
            sb.AppendLine();
            sb.AppendLine($"Opinia: {npc.Opinion}%");
            return sb.ToString().TrimEnd();
        }

        private static string GetAvatarInitial(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "?";

            return name.Trim()[0].ToString().ToUpperInvariant();
        }

        private string BuildDialogIntro()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Strażnik: W okolicy pojawiły się grupy potworów. Twoja pomoc jest niezbędna.");
            sb.AppendLine();
            sb.AppendLine(BuildQuestSummary());
            return sb.ToString().TrimEnd();
        }

        private string BuildQuestSummary()
        {
            if (_session.ActiveQuests.Count == 0)
                return "Brak aktywnych zadań.";

            var sb = new StringBuilder();

            foreach (var quest in _session.ActiveQuests)
            {
                sb.AppendLine($"{quest.Title} – {quest.ProgressDescription}");
                sb.AppendLine($"  {quest.Description}");
                sb.AppendLine($"  Nagroda: {quest.RewardDescription}");
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private string BuildQuestDetailText()
        {
            if (_session.ActiveQuests.Count == 0)
                return "Nie ma otwartych zadań ani patroli do wykonania.";

            var sb = new StringBuilder();
            sb.AppendLine("Szczegóły patroli:");
            sb.AppendLine();

            foreach (var quest in _session.ActiveQuests)
            {
                sb.AppendLine($"• {quest.Title}");
                sb.AppendLine($"  {quest.Description}");
                sb.AppendLine($"  Postęp: {quest.ProgressDescription}");
                sb.AppendLine($"  Nagroda: {quest.RewardDescription}");
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private void CloseDialog()
        {
            dialogOverlay.Visibility = Visibility.Collapsed;
            dialogButtonsPanel.Children.Clear();
            _currentNpcName = null;
        }

        private void TriggerNpcCombatFromDialog()
        {
            var targetName = _currentNpcName ?? "napotkanego NPC";
            CloseDialog();
            Log($"Zaatakowałeś {targetName} z prowokacji!");

            if (!_session.TryStartCombatWithCurrentNpc())
            {
                Log("Nie można obecnie rozpocząć walki.");
                return;
            }

            UpdateEnemyUI();
        }

        public void ShowMap(WorldMap map)
        {
             
        }

        private class QuestListEntry
        {
            public QuestListEntry(Quest quest)
            {
                Title = quest.Title;
                Description = quest.Description;
                Progress = quest.ProgressDescription;
                Reward = quest.RewardDescription;
            }

            public string Title { get; }
            public string Description { get; }
            public string Progress { get; }
            public string Reward { get; }
        }

        private class EquipmentListEntry
        {
            public EquipmentItem Item { get; }

            public EquipmentListEntry(EquipmentItem item)
            {
                Item = item;
            }

            public override string ToString()
            {
                var slot = Item.Slot == EquipmentSlot.Weapon ? "Broń" : "Zbroja";
                return $"{Item.Name} ({slot}, ATK +{Item.ModifyAttack()}, HP +{Item.ModifyHealth()})";
            }
        }

        private class MerchantOfferListEntry
        {
            public MerchantOffer Offer { get; }

            public MerchantOfferListEntry(MerchantOffer offer)
            {
                Offer = offer;
            }

            public override string ToString()
            {
                return $"{Offer.Item.Name} - {Offer.Price} zł";
            }
        }

        private record DialogOption(string Label, Action Action, bool LeadsToCombat = false);
    }
}
