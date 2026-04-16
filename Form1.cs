namespace FileCompare
{
    public partial class Form1 : Form
    {
        private Dictionary<string, FileInfo> leftFiles = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, FileInfo> rightFiles = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, DirectoryInfo> leftDirs = new Dictionary<string, DirectoryInfo>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, DirectoryInfo> rightDirs = new Dictionary<string, DirectoryInfo>(StringComparer.OrdinalIgnoreCase);

        public Form1()
        {
            InitializeComponent();
        }

        private void CompareAndPopulate()
        {
            string leftDir = txtLeftDir.Text;
            string rightDir = txtRightDir.Text;

            bool hasLeft = !string.IsNullOrWhiteSpace(leftDir) && Directory.Exists(leftDir);
            bool hasRight = !string.IsNullOrWhiteSpace(rightDir) && Directory.Exists(rightDir);

            // 두 폴더가 모두 선택되어 있지 않으면 중단
            if (!hasLeft && !hasRight) return;

            lvwLeftDir.BeginUpdate();
            lvwRightDir.BeginUpdate();
            lvwLeftDir.Items.Clear();
            lvwRightDir.Items.Clear();

            try
            {
                leftFiles.Clear();
                leftDirs.Clear();
                if (hasLeft)
                {
                    foreach (var d in Directory.EnumerateDirectories(leftDir))
                        leftDirs[Path.GetFileName(d)] = new DirectoryInfo(d);
                    foreach (var f in Directory.EnumerateFiles(leftDir))
                        leftFiles[Path.GetFileName(f)] = new FileInfo(f);
                }

                rightFiles.Clear();
                rightDirs.Clear();
                if (hasRight)
                {
                    foreach (var d in Directory.EnumerateDirectories(rightDir))
                        rightDirs[Path.GetFileName(d)] = new DirectoryInfo(d);
                    foreach (var f in Directory.EnumerateFiles(rightDir))
                        rightFiles[Path.GetFileName(f)] = new FileInfo(f);
                }

                // 두 폴더의 하위 폴더명 리스트 생성 및 비교
                var allDirNames = leftDirs.Keys.Union(rightDirs.Keys).OrderBy(n => n).ToList();

                foreach (var name in allDirNames)
                {
                    leftDirs.TryGetValue(name, out DirectoryInfo ld);
                    rightDirs.TryGetValue(name, out DirectoryInfo rd);

                    ListViewItem litem = null;
                    ListViewItem ritem = null;

                    // 왼쪽 폴더 아이템 추가
                    if (ld != null)
                    {
                        litem = new ListViewItem(ld.Name);
                        litem.SubItems.Add("<DIR>");
                        litem.SubItems.Add(ld.LastWriteTime.ToString("g"));
                        lvwLeftDir.Items.Add(litem);
                    }

                    // 오른쪽 폴더 아이템 추가
                    if (rd != null)
                    {
                        ritem = new ListViewItem(rd.Name);
                        ritem.SubItems.Add("<DIR>");
                        ritem.SubItems.Add(rd.LastWriteTime.ToString("g"));
                        lvwRightDir.Items.Add(ritem);
                    }

                    // 폴더 상태 결정 및 파일과 동일한 색상 적용
                    if (ld != null && rd != null)
                    { // 양쪽에 모두 있는 경우 (비교)
                        if (ld.LastWriteTime == rd.LastWriteTime)
                        { // 1단계: 동일 폴더 – 양쪽 모두 검은색
                            litem.ForeColor = Color.Black;
                            ritem.ForeColor = Color.Black;
                        }
                        else if (ld.LastWriteTime > rd.LastWriteTime)
                        { // 2단계: 다른 폴더 - 왼쪽이 New(최신), 오른쪽이 Old(과거)
                            litem.ForeColor = Color.Red;
                            ritem.ForeColor = Color.Gray;
                        }
                        else
                        { // 2단계: 다른 폴더 - 왼쪽이 Old(과거), 오른쪽이 New(최신)
                            litem.ForeColor = Color.Gray;
                            ritem.ForeColor = Color.Red;
                        }
                    }
                    else if (ld != null && rd == null)
                    { // 3단계: 단독 폴더 (왼쪽에만 존재) - 보라색
                        litem.ForeColor = Color.Purple;
                    }
                    else if (ld == null && rd != null)
                    { // 3단계: 단독 폴더 (오른쪽에만 존재) - 보라색
                        ritem.ForeColor = Color.Purple;
                    }
                }

                // 두 폴더의 고유한 파일명 리스트 생성 (정렬)
                var allFileNames = leftFiles.Keys.Union(rightFiles.Keys).OrderBy(n => n).ToList();

                foreach (var name in allFileNames)
                {
                    leftFiles.TryGetValue(name, out FileInfo lf);
                    rightFiles.TryGetValue(name, out FileInfo rf);

                    ListViewItem litem = null;
                    ListViewItem ritem = null;

                    // 왼쪽 아이템 추가
                    if (lf != null)
                    {
                        litem = new ListViewItem(lf.Name);
                        litem.SubItems.Add(FormatSizeInKb(lf.Length));
                        litem.SubItems.Add(lf.LastWriteTime.ToString("g"));
                        lvwLeftDir.Items.Add(litem);
                    }

                    // 오른쪽 아이템 추가
                    if (rf != null)
                    {
                        ritem = new ListViewItem(rf.Name);
                        ritem.SubItems.Add(FormatSizeInKb(rf.Length));
                        ritem.SubItems.Add(rf.LastWriteTime.ToString("g"));
                        lvwRightDir.Items.Add(ritem);
                    }

                    // 상태 결정 및 색상 적용
                    if (lf != null && rf != null)
                    { // 양쪽에 모두 있는 경우 (비교)
                        if (lf.LastWriteTime == rf.LastWriteTime)
                        { // 1단계: 동일 파일 – 양쪽 모두 검은색
                            litem.ForeColor = Color.Black;
                            ritem.ForeColor = Color.Black;
                        }
                        else if (lf.LastWriteTime > rf.LastWriteTime)
                        { // 2단계: 다른 파일 - 왼쪽이 New(최신), 오른쪽이 Old(과거)
                            litem.ForeColor = Color.Red;
                            ritem.ForeColor = Color.Gray;
                        }
                        else
                        { // 2단계: 다른 파일 - 왼쪽이 Old(과거), 오른쪽이 New(최신)
                            litem.ForeColor = Color.Gray;
                            ritem.ForeColor = Color.Red;
                        }
                    }
                    else if (lf != null && rf == null)
                    { // 3단계: 단독 파일 (왼쪽에만 존재) - 보라색
                        litem.ForeColor = Color.Purple;
                    }
                    else if (lf == null && rf != null)
                    { // 3단계: 단독 파일 (오른쪽에만 존재) - 보라색
                        ritem.ForeColor = Color.Purple;
                    }
                }

                // 컬럼 너비 자동 조정
                for (int i = 0; i < lvwLeftDir.Columns.Count; i++)
                    lvwLeftDir.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                for (int i = 0; i < lvwRightDir.Columns.Count; i++)
                    lvwRightDir.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lvwLeftDir.EndUpdate();
                lvwRightDir.EndUpdate();
            }
        }

        private string FormatSizeInKb(long bytes)
        {
            return (bytes / 1024.0).ToString("N0") + " KB";
        }

        private void btnLeftDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                // 현재 텍스트박스에 있는 경로를 초기 선택 폴더로 설정
                if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) && Directory.Exists(txtLeftDir.Text))
                {
                    dlg.SelectedPath = txtLeftDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtLeftDir.Text = dlg.SelectedPath;
                    CompareAndPopulate();
                }
            }
        }

        private void btnRightDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                // 현재 텍스트박스에 있는 경로를 초기 선택 폴더로 설정
                if (!string.IsNullOrWhiteSpace(txtRightDir.Text) && Directory.Exists(txtRightDir.Text))
                {
                    dlg.SelectedPath = txtRightDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtRightDir.Text = dlg.SelectedPath;
                    CompareAndPopulate();
                }
            }
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRightDir.Text) || !Directory.Exists(txtRightDir.Text))
            {
                MessageBox.Show(this, "오른쪽(대상) 폴더를 설정해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selected = lvwLeftDir.SelectedItems.Cast<ListViewItem>().ToList();
            if (selected.Count == 0) return;

            bool listUpdated = false;
            foreach (var item in selected)
            {
                var name = item.Text;

                if (leftDirs.TryGetValue(name, out var srcDir))
                { // 선택된 항목이 폴더인 경우
                    var destPath = Path.Combine(txtRightDir.Text, srcDir.Name);
                    if (CopyDirectory(srcDir.FullName, destPath))
                    {
                        listUpdated = true;
                    }
                }
                else if (leftFiles.TryGetValue(name, out var srcFile))
                { // 선택된 항목이 파일인 경우
                    var destPath = Path.Combine(txtRightDir.Text, srcFile.Name);
                    if (CopyFileWithConfirmation(srcFile.FullName, destPath))
                    {
                        listUpdated = true;
                    }
                }
            }

            if (listUpdated) CompareAndPopulate();
        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLeftDir.Text) || !Directory.Exists(txtLeftDir.Text))
            {
                MessageBox.Show(this, "왼쪽(대상) 폴더를 설정해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selected = lvwRightDir.SelectedItems.Cast<ListViewItem>().ToList();
            if (selected.Count == 0) return;

            bool listUpdated = false;
            foreach (var item in selected)
            {
                var name = item.Text;

                if (rightDirs.TryGetValue(name, out var srcDir))
                { // 선택된 항목이 폴더인 경우
                    var destPath = Path.Combine(txtLeftDir.Text, srcDir.Name);
                    if (CopyDirectory(srcDir.FullName, destPath))
                    {
                        listUpdated = true;
                    }
                }
                else if (rightFiles.TryGetValue(name, out var srcFile))
                { // 선택된 항목이 파일인 경우
                    var destPath = Path.Combine(txtLeftDir.Text, srcFile.Name);
                    if (CopyFileWithConfirmation(srcFile.FullName, destPath))
                    {
                        listUpdated = true;
                    }
                }
            }

            if (listUpdated) CompareAndPopulate();
        }

        private bool CopyDirectory(string sourceDir, string destDir)
        {
            try
            {
                if (Directory.Exists(destDir))
                {
                    var srcInfo = new DirectoryInfo(sourceDir);
                    var destInfo = new DirectoryInfo(destDir);

                    // 복사하려는 폴더가 대상 폴더보다 더 오래된 경우 한 번 더 확인
                    if (srcInfo.LastWriteTime < destInfo.LastWriteTime)
                    {
                        var msg = $"'{srcInfo.Name}' 폴더가 기존 폴더보다 오래되었습니다.\n그래도 덮어쓰시겠습니까?";
                        var dr = MessageBox.Show(this, msg, "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.No)
                            return false; // 복사 취소
                    }
                }
                else
                {
                    Directory.CreateDirectory(destDir);
                }

                // 내부 파일 복사
                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    string destPath = Path.Combine(destDir, Path.GetFileName(file));
                    CopyFileWithConfirmation(file, destPath);
                }

                // 내부 폴더 재귀 복사
                foreach (var dir in Directory.GetDirectories(sourceDir))
                {
                    string destPath = Path.Combine(destDir, Path.GetFileName(dir));
                    CopyDirectory(dir, destPath);
                }

                // 폴더 구조 복사 완료 후 대상 폴더의 시간 정보를 원본과 일치시킴
                Directory.SetLastWriteTime(destDir, Directory.GetLastWriteTime(sourceDir));

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"'{Path.GetFileName(sourceDir)}' 폴더 복사 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool CopyFileWithConfirmation(string srcFullName, string destPath)
        {
            try
            {
                if (File.Exists(destPath))
                {
                    var srcInfo = new FileInfo(srcFullName);
                    var destInfo = new FileInfo(destPath);

                    // 복사하려는 파일이 대상 파일보다 더 오래된 경우 한 번 더 확인
                    if (srcInfo.LastWriteTime < destInfo.LastWriteTime)
                    {
                        var msg = $"'{srcInfo.Name}' 파일이 기존 파일보다 오래되었습니다.\n그래도 덮어쓰시겠습니까?";
                        var dr = MessageBox.Show(this, msg, "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.No)
                            return false; // 복사 취소
                    }
                }

                File.Copy(srcFullName, destPath, true); // true: 덮어쓰기 허용
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"'{Path.GetFileName(srcFullName)}' 복사 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
