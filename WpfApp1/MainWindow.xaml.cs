using BibliotekaRPG.map;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace WpfRpg
{
    public partial class MainWindow : Window, IGameEventListener
    {
        private MapSession _session;
        private CancellationTokenSource? _moveCts;

        public MainWindow()
        {
            InitializeComponent();
            InitGame();
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
                {
                    break;
                }

                try 
                {
                    await Task.Delay(200, token);
                }
                catch (TaskCanceledException)
                {
                    
                    return; 
                }

                if (_session.HasActiveBattle)
                {
                    break;
                }
            }
        }

        private void InitGame()
        {
            _session = new MapSession();
            _session.RegisterListener(this);
            _session.Log += Log;
            _session.MapChanged += RenderMap;
            _session.BattleStarted += _ => UpdateEnemyUI();

            playerName.Content = _session.Player.Name;

            UpdatePlayerUI();
            UpdateExpUI();
            RefreshItemsDropdown();
            updateStatsPanel();
            RenderMap();
        }

        private void RenderMap()
        {
            if (_session?.Map == null)
                return;

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
            if (_session != null && _session.PlayerPosition.Row == row && _session.PlayerPosition.Col == col)
                return Brushes.Gold;

            return tile.Type switch
            {
                ITile.TileType.Grass => Brushes.LightGreen,
                ITile.TileType.Forest => Brushes.Green,
                ITile.TileType.Mountain => Brushes.SlateGray,
                ITile.TileType.EnemySpawn => Brushes.DarkRed,
                ITile.TileType.Treasure => Brushes.DarkGoldenrod,
                ITile.TileType.Empty => Brushes.SandyBrown,
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
                _ => ""
            };
        }

        private void TryMovePlayer(int deltaRow, int deltaCol)
        {
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
        }

        public void OnEnemyDefeated(Enemy enemy)
        {
            Log($"Pokonałeś {enemy.Name}!");
            RefreshItemsDropdown();
            UpdatePlayerUI();
            UpdateExpUI();
            updateStatsPanel();
        }

        public void OnPlayerDefeated(Player player)
        {
            Log($"{player.Name} poległ. Git Gut.");
            Close();
        }

        public void OnItemUsed(Character user, IItem item)
        {
            Log($"{user.Name} używa {item.Name}");
            UpdatePlayerUI();
        }

        public void OnEquipmentPutOn(IStatModifier item)
        {
            Log("Założono: " + item.Name);
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
            Log("nie masz już slotów");
        }

        private void updateStatsPanel()
        {
            var player = _session.Player;
            hpStatLabel.Content = $"Hp: {player.Health}/{player.MaxHealth}";
            attackStatLabel.Content = $"Power: {player.AttackPower}";
            levelStatLabel.Content = $"Level: {player.Level}| XP: {player.Experience}/{player.ExperienceToNextLevel}";

            modifiersList.Items.Clear();
            foreach (var m in _session.Equipment.modifiers)
            {
                if (m != null)
                {
                    modifiersList.Items.Add(m.Name);
                }
            }
        }

        private void attackBtn_Click_1(object sender, RoutedEventArgs e)
        {
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

        public void ShowMap(WorldMap map)
        {
            
        }
    }
}