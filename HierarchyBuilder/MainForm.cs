using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Hierarchy;
using  Word = Microsoft.Office.Interop.Word;
using System.IO;


namespace HierarchyBuilder
{
    public partial class MainForm : Form
    {
        protected Word.Application wordapp;

        class NodeStruct
        {
            public NodeStruct(Node _node, TreeNode _treeNode) { node = _node; treeNode = _treeNode; }
            public Node node;
            public TreeNode treeNode;
        }

        Node mainNode;
        TreeNode rootNode;

        Node currentNode;
        int currentNodeIndex;

        List<List<Node>> nodeList;
        List<NodeStruct> nodeStruct;

        int currentLevel = 0;

        public static List<Criterion> criteria = new List<Criterion>();

        string[][] koshargskyUemovCyclesAndObjects = new string[2][];
        string[] koshargskyUemovLevels = new string[] { "Циклы управления", "Объекты управления" };

        List<List<string>> levelsList; // Элементы всех уровней

        List<List<string>> elementsByLevels; //Элементы пренадлежащие конкретному уровню
        string[] levels;    //Уровни

        public static List<Criterion> criterionList = new List<Criterion>();

        public MainForm()
        {
            InitializeComponent();

            radioButtonKoshargskyUemov.Select();

            koshargskyUemovCyclesAndObjects[0] = new string[] {"Оперативное управление (ОУ)", "Организация(ОРГ)", "Преспективное планирование(ПЛ)",
                                                                  "Прогнозирование(ПР)", "Текущее планирование(ТП)", "Учет, Контроль, Анализ(УКА)"};

            koshargskyUemovCyclesAndObjects[1] = new string[] {"Вспомогательное производство (ВП)", "Материально - техническое снабжение(МТС)", "Научно - исследовательская работа(НИР)",
                                                                  "Основное производство(ОП)", "Сбыт продукции(СбП)", "Транспорт(Т)", "Трудовые ресурсы(К)", "Финансы(Ф)"};
            nodeList = new List<List<Node>>();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == 0)
            {
                if (!String.IsNullOrWhiteSpace(textBoxMainTarget.Text))
                {
                    mainNode = new Node(textBoxMainTarget.Text);
                    mainNode.element.weight = 1;

                    if (radioButtonKoshargskyUemov.Checked)
                    {
                        elementsByLevels = new List<List<string>>();
                        for (int i = 0; i < koshargskyUemovCyclesAndObjects.Length; i++)
                            elementsByLevels.Add(new List<string>(koshargskyUemovCyclesAndObjects[i]));

                        levels = koshargskyUemovLevels;

                        tabControl.SelectedIndex += 2;

                        levelsList = new List<List<string>>();
                        for (int i = 0; i < levels.Length; i++)
                            levelsList.Add(new List<string>());

                        for (int i = 0; i < levels.Length; i++)
                            nodeList.Add(new List<Node>());

                        int w = labelLevels.Width;
                        labelLevels.Text = "Элементы уровня \"" + levels[currentLevel] + "\"";
                        labelLevels.Location = new Point(labelLevels.Location.X - labelLevels.Width + w, labelLevels.Location.Y);
                        listBoxLevelsStandart.Items.AddRange(elementsByLevels[currentLevel].ToArray());
                    } else if (radioButtonVoluntaryModel.Checked)
                    {
                        listBoxLevelsStandart.Enabled = true;
                        buttonLevelsAdd.Enabled = true;
                        buttonLevelsAddAll.Enabled = true;
                        tabControl.SelectedIndex++;
                    }
                }
                else
                {
                    MessageBox.Show("Введите цель", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            } else if (tabControl.SelectedIndex == 1)
            {
                if (listBoxVoluntaryLevels.Items.Count < 1)
                {
                    MessageBox.Show("Вы должны добавить хотя бы один уровень", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    levels = new string[listBoxVoluntaryLevels.Items.Count];

                    for (int i = 0; i < levels.Length; i++)
                        nodeList.Add(new List<Node>());

                    levelsList = new List<List<string>>();
                    elementsByLevels = new List<List<string>>();
                    for (int i = 0; i < levels.Length; i++)
                    {
                        elementsByLevels.Add(new List<string>());
                        levelsList.Add(new List<string>());
                    }

                    for (int i = 0; i < listBoxVoluntaryLevels.Items.Count; i++)
                    {
                        levels[i] = listBoxVoluntaryLevels.Items[i].ToString();
                    }

                    listBoxVoluntaryLevels.Items.Clear();

                    int w = labelLevels.Width;
                    labelLevels.Text = "Элементы уровня \"" + levels[currentLevel] + "\"";
                    labelLevels.Location = new Point(labelLevels.Location.X - labelLevels.Width + w, labelLevels.Location.Y);

                    tabControl.SelectedIndex++;
                }

            } else if (tabControl.SelectedIndex == 2)
            {
                if (listBoxLevelsUser.Items.Count == 0)
                {
                    MessageBox.Show("На уровне должен присутствовать хотя бы один элемент", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else
                {
                    if (currentLevel < levels.Length - 1)
                    {
                        int end = listBoxLevelsUser.Items.Count;

                        for (int i = 0; i < end; i++)
                        {
                            levelsList[currentLevel].Add(listBoxLevelsUser.Items[0].ToString());
                            listBoxLevelsUser.Items.RemoveAt(0);
                        }

                        currentLevel++;

                        int w = labelLevels.Width;
                        labelLevels.Text = "Элементы уровня \"" + levels[currentLevel] + "\"";
                        labelLevels.Location = new Point(labelLevels.Location.X - labelLevels.Width + w, labelLevels.Location.Y);

                        if (!radioButtonVoluntaryModel.Checked)
                        {
                            listBoxLevelsStandart.Items.Clear();
                            listBoxLevelsStandart.Items.AddRange(elementsByLevels[currentLevel].ToArray());
                        }  
                    }
                    else
                    {
                        int end = listBoxLevelsUser.Items.Count;

                        for (int i = 0; i < end; i++)
                        {
                            levelsList[currentLevel].Add(listBoxLevelsUser.Items[0].ToString());
                            listBoxLevelsUser.Items.RemoveAt(0);
                        }

                        mainNode.AddChildren(levelsList[0].ToArray());

                        for (int i = 0; i < mainNode.ChildrenCount(); i++)
                            nodeList[0].Add(mainNode[i]);

                        currentNodeIndex = 0;
                        currentLevel = 1;
                        currentNode = nodeList[0][currentNodeIndex];


                        if (levels.Length > 1)
                        {
                            tabControl.SelectedIndex++;

                            labelConnectionsCurrentConnection.Text = "Связи для \"" + currentNode.Parent.Data + " -> " + currentNode.Data + "\"";

                            listBoxConnectionsAvailable.Items.AddRange(levelsList[currentLevel].ToArray());


                        } else
                        {
                            nodesRefresh();
                            tabControl.SelectedIndex += 2;
                        }
                    }
                }

            } else if (tabControl.SelectedIndex == 3)
            {
               
                nodesRefresh();

                tabControl.SelectedIndex++;

            } else if (tabControl.SelectedIndex == 4)
            {
                var hierarchy = Node.GetHierarchy(mainNode);

                var hierarchyString = Node.ListIntoStringArray(hierarchy);

                Draw();
                buttonSavePicture.Enabled = true;
                buttonSavePicture.Visible = true;
                buttonNext.Enabled = false;
                buttonBack.Enabled = true;
                buttonBack.Visible = true;

                tabControl.SelectedIndex++;
            }
        }

        private void buttonLevelsAdd_Click(object sender, EventArgs e)
        {
            if (listBoxLevelsStandart.Items.Count == 0)
            {
                MessageBox.Show("Нет элементов");
            }
            if (listBoxLevelsStandart.SelectedIndex >= 0)
            {
                listBoxLevelsUser.Items.Insert(0, listBoxLevelsStandart.Items[listBoxLevelsStandart.SelectedIndex]);
                listBoxLevelsStandart.Items.RemoveAt(listBoxLevelsStandart.SelectedIndex);
            }
        }

        private void buttonLevelsAddAll_Click(object sender, EventArgs e)
        {
            if (listBoxLevelsStandart.Items.Count == 0)
            {
                MessageBox.Show("Нет элементов");
            }
            int end = listBoxLevelsStandart.Items.Count;

            for (int i = 0; i < end; i++)
            {
                listBoxLevelsUser.Items.Insert(0, listBoxLevelsStandart.Items[0]);
                listBoxLevelsStandart.Items.RemoveAt(0);
            }
        }

        private void buttonLevelsRemove_Click(object sender, EventArgs e)
        {
            if (listBoxLevelsUser.SelectedIndex >= 0)
            {
                if (!radioButtonVoluntaryModel.Checked)
                {
                    listBoxLevelsStandart.Items.Insert(0, listBoxLevelsUser.Items[listBoxLevelsUser.SelectedIndex]);
                }
                else
                {
                    listBoxLevelsStandart.Items.Insert(0, listBoxLevelsUser.Items[listBoxLevelsUser.SelectedIndex]);
                    
                }
                listBoxLevelsUser.Items.RemoveAt(listBoxLevelsUser.SelectedIndex);
            }
        }

        private void buttonLevelsRemoveAll_Click(object sender, EventArgs e)
        {
            int end = listBoxLevelsUser.Items.Count;
            for (int i = 0; i < end; i++)
            {
                if (!radioButtonVoluntaryModel.Checked)
                {
                    listBoxLevelsStandart.Items.Insert(0, listBoxLevelsUser.Items[0]);
                }
                else
                {
                    listBoxLevelsStandart.Items.Insert(0, listBoxLevelsUser.Items[0]);
                  
                }
                listBoxLevelsUser.Items.RemoveAt(0);
            }
        }

        private void buttonLevelsAddUserLvl_Click(object sender, EventArgs e)
        {
            if (!listBoxLevelsUser.Items.Contains(textBoxLevelsUserLvl.Text.Trim()) && 
                !listBoxLevelsStandart.Items.Contains(textBoxLevelsUserLvl.Text.Trim()) &&
                !String.IsNullOrWhiteSpace(textBoxLevelsUserLvl.Text.Trim()))
                listBoxLevelsUser.Items.Add(textBoxLevelsUserLvl.Text.Trim());
            textBoxLevelsUserLvl.Clear();
        }

        private void buttonConnectionsConfirm_Click(object sender, EventArgs e) 
        {
            int end = listBoxConnectionsAdded.Items.Count;
            for (int i = 0; i < end; i++)
            {
                nodeList[currentLevel].Add(currentNode.AddChild(listBoxConnectionsAdded.Items[0].ToString()));
                listBoxConnectionsAdded.Items.RemoveAt(0);
            }

            currentNodeIndex++;
            
            if (currentNodeIndex == nodeList[currentLevel - 1].Count)
            {
                currentNode = nodeList[currentLevel][0];

                currentNodeIndex = 0;
                currentLevel++;
            } else
            {
                currentNode = nodeList[currentLevel-1][currentNodeIndex];
            }

            listBoxConnectionsAvailable.Items.Clear();
            
            if (currentLevel == levels.Length)
            {
                buttonConnectionsConfirm.Enabled = false;
            } else
            {
                labelConnectionsCurrentConnection.Text = "Связи для \"" + currentNode.Parent.Data + " -> " + currentNode.Data + "\"";
                listBoxConnectionsAvailable.Items.AddRange(levelsList[currentLevel].ToArray());
            }
        }

        private void buttonConnectionsAdd_Click(object sender, EventArgs e)
        {
            if (listBoxConnectionsAvailable.SelectedIndex >= 0)
            {
                listBoxConnectionsAdded.Items.Insert(0, listBoxConnectionsAvailable.Items[listBoxConnectionsAvailable.SelectedIndex]);
                listBoxConnectionsAvailable.Items.RemoveAt(listBoxConnectionsAvailable.SelectedIndex);
            }
        }

        private void buttonConnectionsAddAll_Click(object sender, EventArgs e)
        {
            int end = listBoxConnectionsAvailable.Items.Count;

            for (int i = 0; i < end; i++)
            {
                listBoxConnectionsAdded.Items.Insert(0, listBoxConnectionsAvailable.Items[0]);
                listBoxConnectionsAvailable.Items.RemoveAt(0);
            }
        }

        private void buttonConnectionsRemove_Click(object sender, EventArgs e)
        {
            if (listBoxConnectionsAdded.SelectedIndex >= 0)
            {
                listBoxConnectionsAvailable.Items.Insert(0, listBoxConnectionsAdded.Items[listBoxConnectionsAdded.SelectedIndex]);
                listBoxConnectionsAdded.Items.RemoveAt(listBoxConnectionsAdded.SelectedIndex);
            }
        }

        private void buttonConnectionsRemoveAll_Click(object sender, EventArgs e)
        {
            int end = listBoxConnectionsAdded.Items.Count;

            for (int i = 0; i < end; i++)
            {
                listBoxConnectionsAvailable.Items.Insert(0, listBoxConnectionsAdded.Items[0]);
                listBoxConnectionsAdded.Items.RemoveAt(0);
            }
        }

        private void buttonAddLevel_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(textBoxVoluntaryLevel.Text.Trim()))
            {
                if (!listBoxVoluntaryLevels.Items.Contains(textBoxVoluntaryLevel.Text.Trim()))
                    listBoxVoluntaryLevels.Items.Add(textBoxVoluntaryLevel.Text.Trim());
            }

            textBoxVoluntaryLevel.Clear();
        }

        private void buttonRemoveLevel_Click(object sender, EventArgs e)
        {
            if (listBoxVoluntaryLevels.SelectedIndex >= 0)
            {
                listBoxVoluntaryLevels.Items.RemoveAt(listBoxVoluntaryLevels.SelectedIndex);
            }
        }

        private void listBoxLevelsStandart_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxLevelsStandart.SelectedIndex >= 0)
            {
                listBoxLevelsUser.Items.Insert(0, listBoxLevelsStandart.Items[listBoxLevelsStandart.SelectedIndex]);
                listBoxLevelsStandart.Items.RemoveAt(listBoxLevelsStandart.SelectedIndex);
            }
        }

        private void listBoxLevelsUser_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxLevelsUser.SelectedIndex >= 0)
            {
                if (!radioButtonVoluntaryModel.Checked)
                {
                    listBoxLevelsStandart.Items.Insert(0, listBoxLevelsUser.Items[listBoxLevelsUser.SelectedIndex]);
                }
                listBoxLevelsUser.Items.RemoveAt(listBoxLevelsUser.SelectedIndex);
            }
        }

        private void listBoxConnectionsAvailable_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxConnectionsAvailable.SelectedIndex >= 0)
            {
                listBoxConnectionsAdded.Items.Insert(0, listBoxConnectionsAvailable.Items[listBoxConnectionsAvailable.SelectedIndex]);
                listBoxConnectionsAvailable.Items.RemoveAt(listBoxConnectionsAvailable.SelectedIndex);
            }
        }

        private void listBoxConnectionsAdded_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxConnectionsAdded.SelectedIndex >= 0)
            {
                listBoxConnectionsAvailable.Items.Insert(0, listBoxConnectionsAdded.Items[listBoxConnectionsAdded.SelectedIndex]);
                listBoxConnectionsAdded.Items.RemoveAt(listBoxConnectionsAdded.SelectedIndex);
            }
        }

        private void buttonSavePicture_Click(object sender, EventArgs e)
        {
            if (pictureBoxHierarchy.Image != null)
            {
                SaveFileDialog savedialog = new SaveFileDialog();
                savedialog.Title = "Сохранить картинку как...";
                savedialog.OverwritePrompt = true;
                savedialog.CheckPathExists = true;
                savedialog.Filter = "Image Files(*.JPG)|*.JPG";
                savedialog.ShowHelp = true;

                System.Drawing.Imaging.ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;

                if (savedialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBoxHierarchy.Image.Save(savedialog.FileName, jpgEncoder, myEncoderParameters);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void nodesMatching(Node mainNode, TreeNode rootNode)
        {
            for (int i = 0; i < mainNode.ChildrenCount(); i++)
            {
                TreeNode tempNode = new TreeNode(mainNode[i].Data + " {" + mainNode[i].element.weight + "} ");
                rootNode.Nodes.Add(tempNode);

                nodeStruct.Add(new NodeStruct(mainNode[i], tempNode));

                nodesMatching(mainNode[i], tempNode);
            }
        }

        private int GetDeepestChildNodeLevel(TreeNode node)
        {
            var subLevel = node.Nodes.Cast<TreeNode>().Select(GetDeepestChildNodeLevel);
            return subLevel.Count() == 0 ? 1 : subLevel.Max() + 1;
        }

        private void nodesRefresh()
        {
            nodeStruct = new List<NodeStruct>();

            rootNode = new TreeNode(mainNode.Data + " {" + mainNode.element.weight + "} ");

            nodeStruct.Add(new NodeStruct(mainNode, rootNode));

            nodesMatching(mainNode, rootNode);

            treeViewHierarchy.Nodes.Clear();

            treeViewHierarchy.Nodes.Add(rootNode);

            treeViewHierarchy.Refresh();

            treeViewHierarchy.ExpandAll();
        }

        public static System.Drawing.Imaging.ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            return System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders().Where(x => x.FormatID == format.Guid).FirstOrDefault();
        }
        private int SearchCountWords(string Str) // Нахождение количаства слов в строке
        {
            char[] ArrStr = Str.ToCharArray(0, Str.Length);

            int CountWords = 1; // Количество слов в строке

            for (int i = 0; i < Str.Length; i++) // Узнаем количество слов в строке
            {
                if (ArrStr[i] == 32)
                    CountWords++;
            }

            return CountWords;
        }

        private int SearchMaxWordLength(string Str) // Поиск максимальной длины слова
        {
            char[] ArrStr = Str.ToCharArray(0, Str.Length);

            int CountWords = SearchCountWords(Str); // Количество слов в строке
            int MaxWordLength = 0; // Максимальная длина слова

            string[] StrWords = new string[CountWords]; // Создаем массив из слов строки

            StrWords = Str.Split(' ');

            for (int i = 0; i < CountWords; i++) // Узнаем максимальную длину слова
            {
                if (StrWords[i].Length > MaxWordLength)
                    MaxWordLength = StrWords[i].Length;
            }

            return MaxWordLength;
        }


        private int SearchCountSubstr(string Str)
        {
            int CountSubstr = 0;
            char[] ArrStr = Str.ToCharArray(0, Str.Length);
            for (int i = 0; i < Str.Length; i++)
            {
                if (ArrStr[i] == '\n')
                {
                    CountSubstr++;
                }
            }
            return CountSubstr + 1;

        }

        private string LineBreaks(string Str, int LengthSubstr) // Разбиение строки на подстроки с помощью вставки переносов
        {
            int CountWords = SearchCountWords(Str);
            string[] StrWords = new string[CountWords]; // Создаем массив из слов строки

            StrWords = Str.Split(' ');

            string NewStr = StrWords[0];

            int StrLength = NewStr.Length; // Текущая длина подстроки

            for (int i = 0; i < CountWords - 1; i++) // Вставляем переносы на следующую строку
            {
                if (StrLength + StrWords[i + 1].Length + " ".Length <= LengthSubstr)
                {
                    NewStr = NewStr + " " + StrWords[i + 1];
                    StrLength += (StrWords[i + 1].Length + " ".Length);
                }
                else
                {
                    NewStr = NewStr + "\n" + StrWords[i + 1];
                    StrLength = StrWords[i + 1].Length;
                }
            }

            return NewStr;
        }
        private void drawCell(Graphics schematic, int cellWidth, int cellHeight, int x, int y, string str, int strIndent)
        {
            SolidBrush element = new SolidBrush(Color.Cornsilk); // Цвет фона окн элементов 
            Font textFont = new Font("Times New Roman", 10);

            schematic.FillRectangle(element, new Rectangle(x, y, cellWidth, cellHeight));
            schematic.DrawRectangle(new Pen(Color.Black), new Rectangle(x, y, cellWidth, cellHeight));
            schematic.DrawString(str, textFont, Brushes.Black, new Point(x + strIndent, y + strIndent));
        }
        private void Draw()
        {
            int width = 0, height = 0;
            int indent = 2;
            int wCount = 0, hCount = 0, wMaxLength = 0;

            foreach (List<Node> list in nodeList)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Data.Length > wMaxLength)
                        wMaxLength = list[i].Data.Length;
                }

                if (list.Count > wCount)
                    wCount = list.Count;
            }

            hCount = nodeList.Count;

            int cellWidth;
            int cellHeight;
            int spacer;

            // В каждой из ячеек ищем слово максимальной длины:
            int MaxWordLength = SearchMaxWordLength(mainNode.Data);
            for (int i = 0; i < nodeList.Count; i++)
            {
                for (int j = 0; j < nodeList[i].Count; j++)
                {
                    if (SearchMaxWordLength(nodeList[i][j].Data) > MaxWordLength)
                        MaxWordLength = SearchMaxWordLength(nodeList[i][j].Data);
                }
            }

            // Разбиваем строки на подстроки, и ищем максимальное количество подстрок
            string mainData = LineBreaks(mainNode.Data, MaxWordLength);
            string[][] arrayData = new string[nodeList.Count][];
            for (int i = 0; i < arrayData.Length; i++)
            {
                arrayData[i] = new string[nodeList[i].Count];
            }

            int MaxSubstring = SearchCountSubstr(mainData);
            for (int i = 0; i < nodeList.Count; i++)
            {
                for (int j = 0; j < nodeList[i].Count; j++)
                {
                    arrayData[i][j] = LineBreaks(nodeList[i][j].Data, MaxWordLength);
                    if (SearchCountSubstr(arrayData[i][j]) > MaxSubstring)
                        MaxSubstring = SearchCountSubstr(arrayData[i][j]);
                }
            }
            MaxSubstring++; // Строка с весом элемента

            cellWidth = MaxWordLength * 6 + 20;
            cellHeight = MaxSubstring * 13 + 20;

            spacer = cellWidth / 3;

            width = cellWidth * wCount + spacer * (wCount - 1) + 2 * indent;        // - ширина пространства для рисования
            height = cellHeight * (hCount + 1) + spacer * (hCount) + 2 * indent;    // - высота пространства для рисования

            int[] arrChildrenX = new int[nodeList[nodeList.Count - 1].Count];

            int coordinateX = indent;
            for (int i = 0; i < arrChildrenX.Length;)
            {
                for (int j = 0; j < nodeList[nodeList.Count - 2].Count; j++)
                {
                    if (nodeList[nodeList.Count - 2][j].ChildrenCount() > 0)
                    {
                        for (int k = 0; k < nodeList[nodeList.Count - 2][j].ChildrenCount(); k++)
                        {
                            arrChildrenX[i] = coordinateX;
                            coordinateX += cellWidth + spacer;
                            i++;
                        }
                    }
                    else
                    {
                        //if ((nodeList[nodeList.Count - 2][j - 1].ChildrenCount() < 3) && j > 0)
                            coordinateX += cellWidth + spacer;
                    }
                }
            }

            if (coordinateX + cellWidth + indent > width)
                width = coordinateX + cellWidth + indent;

            Bitmap flag = new Bitmap(width, height);

            Pen blackLine = new Pen(Color.Black);

            Graphics schematic = Graphics.FromImage(flag);

            SolidBrush background = new SolidBrush(Color.White);

            schematic.FillRectangle(background, new Rectangle(0, 0, width, height));

            // Рисование ячейки с главной целью:
            int x = width / 2 - cellWidth / 2, y = indent;

            drawCell(schematic, cellWidth, cellHeight, x, y, mainData + "\n{" + mainNode.element.weight + "}", indent);
            schematic.DrawLine(blackLine, x + cellWidth / 2, y + cellHeight, x + cellWidth / 2, y + cellHeight + spacer / 2);

            int[] arrParentX = new int[nodeList[nodeList.Count - 2].Count];

            for (int i = nodeList.Count - 1; i >= 0; i--)
            {
                if (i >= 1)
                {
                    for (int j = 0; j < nodeList[i - 1].Count; j++)
                    {
                        arrParentX[j] = 0;
                        for (int k = 0; k < nodeList[i].Count; k++)
                        {
                            if (nodeList[i][k].Parent == nodeList[i - 1][j])
                                arrParentX[j] += arrChildrenX[k];
                        }
                        if (nodeList[i - 1][j].ChildrenCount() > 0)
                            arrParentX[j] = arrParentX[j] / nodeList[i - 1][j].ChildrenCount();
                        else
                            if (j > 0)
                            arrParentX[j] = arrParentX[j - 1] + cellWidth + spacer;
                    }
                }

                int indexParent = 0; // порядковый номер родителя
                for (int j = 0; j < nodeList[i].Count; j++)
                {
                    // schematic.DrawLine(blackLine, parentX, parentY, parentX, parentY + spacer / 2);
                    int ChildrenY = indent + (i + 1) * (cellHeight + spacer);
                    drawCell(schematic, cellWidth, cellHeight, arrChildrenX[j], ChildrenY, arrayData[i][j] + "\n{" + nodeList[i][j].element.weight + "}", indent);

                    schematic.DrawLine(blackLine, arrChildrenX[j] + cellWidth / 2, ChildrenY, arrChildrenX[j] + cellWidth / 2, ChildrenY - spacer / 2);

                    // Ищем, какой X у родителя:
                    if (i >= 1)
                    {
                        for (int k = 0; k < nodeList[i - 1].Count; k++)
                        {
                            if (nodeList[i][j].Parent == nodeList[i - 1][k])
                                indexParent = k;
                        }
                        schematic.DrawLine(blackLine, arrParentX[indexParent] + cellWidth / 2, ChildrenY - spacer, arrParentX[indexParent] + cellWidth / 2, ChildrenY - spacer / 2);
                    }
                    schematic.DrawLine(blackLine, arrChildrenX[j] + cellWidth / 2, ChildrenY - spacer / 2, arrParentX[indexParent] + cellWidth / 2, ChildrenY - spacer / 2);
                }

                if (i >= 1)
                {
                    Array.Resize(ref arrChildrenX, nodeList[i - 1].Count);
                    for (int j = 0; j < arrChildrenX.Length; j++)
                    {
                        arrChildrenX[j] = arrParentX[j];
                    }
                }
            }

            pictureBoxHierarchy.Image = flag;
            flowLayoutPanel.AutoScroll = true;
        }

        private void buttonSetPrice_Click(object sender, EventArgs e)
        {
            int depth = 0;
            depth = GetDeepestChildNodeLevel(rootNode) - 1;

            if (listBoxCriterion.Items.Count > 0)
            {
                if (treeViewHierarchy.SelectedNode.Index >= 0 && treeViewHierarchy.SelectedNode.Level != depth)
                {
                    criterionList.Clear();

                    for (int i = 0; i < listBoxCriterion.Items.Count; i++)
                    {
                        criterionList.Add(new Criterion(listBoxCriterion.Items[i].ToString()));
                    }

                    for (int i = 0; i < nodeStruct.Count; i++)
                        if (nodeStruct[i].treeNode == treeViewHierarchy.SelectedNode)
                        {
                            ItemPrice f = new ItemPrice(ref nodeStruct[i].node, ref criterionList);
                            f.Owner = this;
                            f.Text = "Рассчёт весов для \"" + nodeStruct[i].node.Data + "\"";
                            f.ShowDialog();
                            break;
                        }

                    nodesRefresh();

                    treeViewHierarchy.SelectedNode = treeViewHierarchy.Nodes[0];
                }
                else
                {
                    MessageBox.Show("Выберите узел для оценки, нижний уровень не может быть оценён", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Должен быть указан хотя бы один критерий", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonCriterionAdd_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(textBoxCriterion.Text.Trim()))
            {
                if (!listBoxCriterion.Items.Contains(textBoxCriterion.Text.Trim()))
                    listBoxCriterion.Items.Add(textBoxCriterion.Text.Trim());
            }

            textBoxCriterion.Clear();
        }

        private void buttonCriterionRemove_Click(object sender, EventArgs e)
        {
            if (listBoxCriterion.SelectedIndex >= 0)
            {
                listBoxCriterion.Items.RemoveAt(listBoxCriterion.SelectedIndex);
            }
        }

        private void buttonCriterionRemoveAll_Click(object sender, EventArgs e)
        {
            listBoxCriterion.Items.Clear();
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            nodesRefresh();

            buttonSavePicture.Enabled = false;
            buttonSavePicture.Visible = false;
            buttonNext.Enabled = true;
            buttonBack.Enabled = false;
            buttonBack.Visible = false;

            tabControl.SelectedIndex--;
        }

        private void textBoxVoluntaryLevel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonAddLevel_Click(sender,e);
            }
        }

        private void textBoxLevelsUserLvl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonLevelsAddUserLvl_Click(sender, e);
            }
        }

        private void textBoxCriterion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonCriterionAdd_Click(sender, e);
            }
        }

        private void wordToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void wordDocToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wordapp = new Word.Application();

            wordapp.Visible = true;
           
            Object filename = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDi‌​rectory, "..\\..\\")) + @"Resources\Help of HierarchyBuilder.docx";
            
            Word.Document document = wordapp.Documents.Open(ref filename);
        }

        private void разработчикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Над программой работали:\tАнастасия Сергеевна Перелыгина 23502/3\n\t\t\tАнатолий Сергеевич Колесников 23502/2\n\t\t\tГригорий Андреевич Скобелев 23502/2\n2016 год.");
        }

    
    }
}
