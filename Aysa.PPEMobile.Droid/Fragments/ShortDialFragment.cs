using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using Aysa.PPEMobile.Droid.Activities;
using Newtonsoft.Json;
using Android.Views.InputMethods;




namespace Aysa.PPEMobile.Droid.Fragments
{

	public class ShortDialFragment : global::AndroidX.AppCompat.App.AppCompatDialogFragment
	{

		ExpandableListView listView;
		LevelListAdapter mAdapter;
		SearchSectionListAdapter mSearchAdapter;
		Boolean isSearching = false;

		public static ShortDialFragment newInstance()
		{
			Bundle args = new Bundle();
			ShortDialFragment fragment = new ShortDialFragment();
			fragment.Arguments = args;
			return fragment;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);


		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			System.Diagnostics.Debug.WriteLine("ShortDialFragment.OnCreateView");
			View view = inflater.Inflate(Resource.Layout.ShortDialFragment, null);
			InitTableView(view);

			GetLevels();

			//clear search textfield
			Button cleanButton = view.FindViewById<Button>(Resource.Id.cleanEventTextFilter);
			cleanButton.Click += (sender, e) =>
			{
				EditText eventSearched = view.FindViewById<EditText>(Resource.Id.editTextSearch);
				eventSearched.Text = "";
			};

			// Search EditText
			EditText editSearch = view.FindViewById<EditText>(Resource.Id.editTextSearch);
			editSearch.EditorAction += (object sender, TextView.EditorActionEventArgs e) =>
			{

				if (e.ActionId == ImeAction.Done)
				{
					EditText et = sender as EditText;
					if (et.Text.Length > 0)
					{
						SearchSectionsByName(et.Text);
					}
					else
					{
						// Return to default levels
						listView.SetAdapter(mAdapter);
					}


				}
				// e.Handled = false;

			};

			editSearch.TextChanged += (o, e) =>
			{
				if (editSearch.Text.Length == 0)
				{
					listView.SetAdapter(mAdapter);
				}
			};

			return view;
		}

		#region Private Metods

		// Handler for the item click event:
		void OnItemClick(object sender, Section sectionSelected)
		{

			// Pass section selected to ShortDialDetailsActivity
			Intent shortDialDetails = new Intent(Application.Context, typeof(ShortDialDetailsActivity));
			shortDialDetails.PutExtra("sectionSelected", JsonConvert.SerializeObject(sectionSelected));

			StartActivity(shortDialDetails);
		}

		void OnItemSearchedClick(object sender, Section sectionSelected)
		{

			// Pass section selected to ShortDialDetailsActivity
			Intent shortDialDetails = new Intent(Application.Context, typeof(ShortDialDetailsActivity));
			shortDialDetails.PutExtra("sectionSelected", JsonConvert.SerializeObject(sectionSelected));

			StartActivity(shortDialDetails);
		}

		private void InitTableView(View view)
		{
			// Get our RecyclerView layout:
			listView = view.FindViewById<ExpandableListView>(Resource.Id.recyclerView);

		}

		private void GetLevels()
		{
			System.Diagnostics.Debug.WriteLine("    ShortDialFragment.GetLevels");
			Task.Run(async () =>
			{

				try
				{

					List<Level> LevelList = await AysaClient.Instance.GetLevels();

					this.Activity.RunOnUiThread(() =>
					{
						System.Diagnostics.Debug.WriteLine("        ShortDialFragment.GetLevels.RunOnUiThread");
						// Create an adapter for the RecyclerView, and pass it the
						// data set (the photo album) to manage:
						mAdapter = new LevelListAdapter(LevelList, Context);

						// Register the item click handler (below) with the adapter:
						mAdapter.ItemClick += OnItemClick;
						mAdapter.FavoriteClick += AddOrDeleteFavorite;

						// Plug the adapter into the RecyclerView:
						listView.SetAdapter(mAdapter);
					});

				}
				catch (HttpUnauthorized)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowSessionExpiredError();
					});
				}
				catch (NetworkConnectionException)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
					});
				}
				catch (Exception ex)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
					});
				}
				finally
				{
				}
			});
		}

		private void AddOrDeleteFavorite(object sender, Section section)
		{
			//System.Diagnostics.Debug.WriteLine("    ShortDialFragment.STAR-FAVORITE");
			Task.Run(async () =>
			{
				try
				{

					await AysaClient.Instance.AddOrDeleteFavorite(section.Id ,section.Favorito);

				}
				catch (HttpUnauthorized)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowSessionExpiredError();
					});
				}
				catch (NetworkConnectionException)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
					});
				}
				catch (Exception ex)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
					});
				}
				finally
				{
					GetLevels();
				}
			});
		}

		private void SearchSectionsByName(string text)
		{

			Task.Run(async () =>
			{

				try
				{

					if (isSearching)
					{
						return;
					}
					isSearching = true;

					List<Section> sections = await AysaClient.Instance.SearchSectionByName(text);

					this.Activity.RunOnUiThread(() =>
					{

						mSearchAdapter = new SearchSectionListAdapter(sections);
						mSearchAdapter.ItemClick += OnItemSearchedClick;
						listView.SetAdapter(mSearchAdapter);
					});

				}
				catch (HttpUnauthorized)
				{
					this.Activity.RunOnUiThread(() =>
					{
						ShowSessionExpiredError();
					});
				}
				catch (Exception ex)
				{
					var message = ex.Message;

					if (message == "")
					{
						message = GetString(Resource.String.unexpected_error);
					}

					this.Activity.RunOnUiThread(() =>
					{
						ShowErrorAlert(message);
					});

				}
				finally
				{
					isSearching = false;
					// Dismiss an Activity Indicator in the status bar

				}
			});

		}

		private void ShowErrorAlert(string message)
		{
			AlertDialog.Builder alert = new AlertDialog.Builder(this.Activity);
			alert.SetTitle("Error");
			alert.SetMessage(message);

			Dialog dialog = alert.Create();
			dialog.Show();
		}

		private void ShowSessionExpiredError()
		{
			AlertDialog.Builder dialog = new AlertDialog.Builder(this.Activity);
			AlertDialog alert = dialog.Create();
			alert.SetTitle("Aviso");
			alert.SetMessage("Su sesión ha expirado");
			alert.SetButton("Aceptar", (c, ev) =>
			{
				Intent shortDialDetails = new Intent(Application.Context, typeof(LoginActivity));
				StartActivity(shortDialDetails);
				this.Activity.Finish();
			});

			alert.Show();
		}

		#endregion

	}



	//----------------------------------------------------------------------
	// ADAPTER
	public class LevelListAdapter : BaseExpandableListAdapter
	{
		// Event handler for item clicks:
		public event EventHandler<Section> ItemClick;

		public Context context;
		public List<Level> mLevelList;

		public event EventHandler<Section> FavoriteClick;

		public LevelListAdapter(List<Level> photoAlbum, Context context)
		{
			mLevelList = photoAlbum;
			this.context = context;
		}



		public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
		{
			View header = convertView;
			if (header == null)
			{
				var inflater = context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
				header = inflater.Inflate(Resource.Layout.LevelRow, null);
			}
			if (isExpanded)
			{
				header.SetBackgroundResource(Resource.Drawable.rounded_topCorners_background);
			}
			else
			{
				header.SetBackgroundResource(Resource.Drawable.rounded_corners_background);
				header.FindViewById<View>(Resource.Id.levelSeparator).Visibility = ViewStates.Visible;
			}

			header.FindViewById<TextView>(Resource.Id.txtLevelTitle).Text = mLevelList[groupPosition].Nombre;

			return header;
		}

		public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
		{
			View row = convertView;
			Section section = mLevelList[groupPosition].Sectores[childPosition];

			var inflater = context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
			row = inflater.Inflate(Resource.Layout.SectionRow, null);
			row.FindViewById<TextView>(Resource.Id.txtSectionTitle).Text = section?.Nombre;

			ImageButton favoriteButton = row.FindViewById<ImageButton>(Resource.Id.favoriteButton);
			favoriteButton.SetImageResource(section!=null&&section.Favorito ? Resource.Drawable.star : Resource.Drawable.star_empty);
			//favoriteButton.SetCompoundDrawablesWithIntrinsicBounds(0, 0, section.Favorito ? Resource.Drawable.star : Resource.Drawable.star_empty, 0);

			favoriteButton.Click += (sender, e) => OnClickFavorite(section, groupPosition, favoriteButton);
			row.Click += (sender, e) => OnClick(section);

			return row;

		}

		void OnClickFavorite(Section section, int groupPosition, ImageButton favoriteButton)
		{
			favoriteButton.SetImageResource(!(section!=null&&section.Favorito)? Resource.Drawable.star : Resource.Drawable.star_empty);
			FavoriteClick?.Invoke(this, section);
			mLevelList[groupPosition].Sectores.Remove(section);
		}

		public override int GetChildrenCount(int groupPosition)
		{
			if (mLevelList[groupPosition].Sectores != null)
			{
				return mLevelList[groupPosition].Sectores.Count;
			}
			else
			{

				return 0;
			}

		}

		public override int GroupCount
		{
			get
			{
				return mLevelList.Count;
			}
		}

		public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
		{
			throw new NotImplementedException();
		}

		public override long GetChildId(int groupPosition, int childPosition)
		{
			return childPosition;
		}

		public override Java.Lang.Object GetGroup(int groupPosition)
		{
			throw new NotImplementedException();
		}

		public override long GetGroupId(int groupPosition)
		{
			return groupPosition;
		}

		public override bool IsChildSelectable(int groupPosition, int childPosition)
		{
			return true;
		}

		public override bool HasStableIds
		{
			get
			{
				return true;
			}
		}


		// Raise an event when the item-click takes place:
		void OnClick(Section section)
		{
			if (ItemClick != null)
				ItemClick(this, section);
		}
	}


	// SEARCH SECTIONS


	public class SearchSectionListAdapter : BaseExpandableListAdapter
	{

		public event EventHandler<Section> ItemClick;

		public List<Section> mSectionList;


		public SearchSectionListAdapter(List<Section> sections)
		{
			mSectionList = sections;
		}

		public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
		{
			View row = convertView;
			if (row == null)
			{
				var inflater = LayoutInflater.From(parent.Context);
				row = inflater.Inflate(Resource.Layout.SectionSearchedRow, null);
			}
			row.FindViewById<TextView>(Resource.Id.txt_title).Text = mSectionList[groupPosition].Nombre;


			row.Click += (sender, e) => OnClick(mSectionList[groupPosition]);

			return row;
		}

		public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
		{
			View row = convertView;


			return row;
		}

		public override int GetChildrenCount(int groupPosition)
		{
			return 0;
		}

		public override int GroupCount
		{
			get
			{
				return mSectionList.Count;
			}
		}

		public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
		{
			throw new NotImplementedException();
		}

		public override long GetChildId(int groupPosition, int childPosition)
		{
			return 0;
		}

		public override Java.Lang.Object GetGroup(int groupPosition)
		{
			throw new NotImplementedException();
		}

		public override long GetGroupId(int groupPosition)
		{
			return groupPosition;
		}

		public override bool IsChildSelectable(int groupPosition, int childPosition)
		{
			return true;
		}

		public override bool HasStableIds
		{
			get
			{
				return true;
			}
		}


		void OnClick(Section section)
		{
			if (ItemClick != null)
				ItemClick(this, section);
		}
	}

}
