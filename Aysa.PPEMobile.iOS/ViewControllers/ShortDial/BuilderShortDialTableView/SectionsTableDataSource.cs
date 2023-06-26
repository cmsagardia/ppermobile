using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Collections.Generic;
using Aysa.PPEMobile.Model;

namespace Aysa.PPEMobile.iOS.ViewControllers.ShortDial.BuilderShortDialTableView
{
    public class SectionsTableDataSource:  UITableViewSource
    {

        string CellIdentifier = "SectionTableViewCell";

        // Height for TableViewCellHeader
        public static readonly int HeightHeaderView = 50;

        // Get reference of parentViewController
        private ShortDialViewController Owner;
        private List<Level> Levels;
        private bool[] _isSectionOpen;
        private EventHandler _headerButtonCommand;


        public SectionsTableDataSource(List<Level> levels, ShortDialViewController owner,UITableView tableView)
        {
            this.Owner = owner;
            this.Levels = levels;

            _isSectionOpen = new bool[levels.Count];

            tableView.RegisterNibForCellReuse(UINib.FromName(SectionTableViewCell.Key, NSBundle.MainBundle), SectionTableViewCell.Key);
            tableView.RegisterNibForHeaderFooterViewReuse(UINib.FromName(HeaderLevelCell.Key, NSBundle.MainBundle), HeaderLevelCell.Key);

            _headerButtonCommand = (sender, e) =>
            {
                var button = sender as UIButton;
                var section = button.Tag;
                _isSectionOpen[(int)section -1] = !_isSectionOpen[(int)section -1];


                // Animate the section cells
                var paths = new NSIndexPath[RowsInSection(tableView, section)];
                for (int i = 0; i < paths.Length; i++)
                {
                    paths[i] = NSIndexPath.FromItemSection(i, section);
                }

                owner.ReloadTableView(paths,section);
            };
        }


        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            if (section == 0) { return 5; }
            return 44f;
        }

        public override nfloat EstimatedHeightForHeader(UITableView tableView, nint section)
        {
            return 44f;
        }


        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 88f;
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {

            // It's to simulate top padding between UISerchBar and first section 
            if (section == 0){return null; }

            HeaderLevelCell header = tableView.DequeueReusableHeaderFooterView(HeaderLevelCell.Key) as HeaderLevelCell;
            header.LoadLevelInView(Levels[(int)section - 1]);
            foreach (var view in header.Subviews)
            {
                if (view is HiddenHeaderButton)
                {
                    view.RemoveFromSuperview();
                }
            }
            var hiddenButton = CreateHiddenHeaderButton(header.Bounds, section);
            header.AddSubview(hiddenButton);

            return header;
        }

        private HiddenHeaderButton CreateHiddenHeaderButton(CGRect frame, nint tag)
        {
            var button = new HiddenHeaderButton(frame);
            button.Tag = tag;
            button.TouchUpInside += _headerButtonCommand;
            return button;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return Levels.Count + 1;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            // It's to simulate top padding between UISerchBar and first section
            if (section == 0) { return 0; }
            Level level = Levels[(int)section - 1];
            return _isSectionOpen[(int)section - 1] ? level.Sectores.Count : 0;


        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {

            var cell = tableView.DequeueReusableCell(CellIdentifier) as SectionTableViewCell;

            // Remove the cell highlight color
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            Level level = Levels[indexPath.Section - 1]; // -1 it's for the extra padding
            cell.LoadSectionInView(level.Sectores[indexPath.Row], Owner);
            // Round corners to the last cell to simulate table group style of ios 6.0
            if (indexPath.Row == level.Sectores.Count - 1)
            {
                // Round bottom corners for the last row of tableView
                cell.isLastCellInSection = true;
            }

            return cell;

        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {

            // Get section selected
            Level level = Levels[indexPath.Section - 1];// -1 it's for the extra padding
            Section sectionObj = level.Sectores[indexPath.Row];


            Owner.SectionSelectedFromTableView(sectionObj);

        }

    }
}


public class HiddenHeaderButton : UIButton
{
    public HiddenHeaderButton(CGRect frame) : base(frame)
    {

    }
}

public class ExpandableTableModel<T> : List<T>
{
    public string Title { get; set; }
    public ExpandableTableModel(IEnumerable<T> collection) : base(collection) { }
    public ExpandableTableModel() : base() { }
}
