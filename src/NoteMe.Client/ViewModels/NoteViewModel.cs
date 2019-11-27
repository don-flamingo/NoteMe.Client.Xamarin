using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using N.Publisher;
using NoteMe.Client.Domain.Notes;
using NoteMe.Client.Domain.Notes.Messages;
using NoteMe.Client.Domain.Notes.Queries;
using NoteMe.Common.DataTypes.Enums;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace NoteMe.Client.ViewModels
{
    public class NoteViewModel : ViewModelBase
    {
        private readonly NSubscription _newNotesSubscription;
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();
        public ICommand SelectNoteCommand { get; }

        protected NoteViewModel(IViewModelFacade viewModelFacade) : base(viewModelFacade)
        {
            SelectNoteCommand = new Command<Note>(async (x) => await GoToNoteAsync(x));
            
            _newNotesSubscription = NPublisher.SubscribeIt<NewNotesMessage>(message =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var note in message.Notes.Where(x => x.Status == StatusEnum.Normal))
                    {
                        Notes.Insert(0, note);
                    }
                    
                    OnPropertyChanged(nameof(Note.CreatedAt));
                    OnPropertyChanged(nameof(Note.Name));
                });
            });
        }

        private Task GoToNoteAsync(Note note)
            => NavigateTo($"//notes/details?id={note.Id}");

        public override async Task InitializeAsync(object parameter = null)
        {
            await base.InitializeAsync(parameter);
            
            var query = new GetActiveNotesQuery();
            var notes = await DispatchQueryAsync<GetActiveNotesQuery, ICollection<Note>>(query);

            foreach (var note in notes)
            {
                Notes.Add(note);
            }
        }
    }
}