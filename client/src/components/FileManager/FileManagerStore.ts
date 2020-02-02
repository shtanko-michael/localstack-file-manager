import { observable, action, runInAction, reaction } from 'mobx';
import { Item } from 'models';
import { AppSettings } from 'appsettings';
import { inject } from 'react-ioc';

const withQuery = (url: string, params: any) => {
  let query = Object.keys(params)
    .filter(k => params[k] !== undefined)
    .map(k => encodeURIComponent(k) + '=' + encodeURIComponent(params[k]))
    .join('&');
  url += (url.indexOf('?') === -1 ? '?' : '&') + query;
  return url;
};



export default class FileManagerStore {
  @inject(AppSettings) settings: AppSettings;

  @observable breadcrumb: Item[] = [];
  @observable items: Item[] = [];
  @observable fetching: boolean = false;
  @observable currentFolder: Item | null = null;

  @observable folderName: string = '';
  @action setFolderName(val: string) { this.folderName = val; }

  constructor() {
    reaction(() => this.currentFolder, () => this.fetch(), {
      fireImmediately: true
    });
  }

  @action async fetch() {
    this.fetching = true;
    let result = await fetch(withQuery(`${this.settings.api}/file`, { parentId: this.currentFolder?.id }));
    let json = await result.json();
    runInAction(() => {
      this.items = json.items.map(Item.fromJson);
      this.breadcrumb = json.breadcrumb.map(Item.fromJson)
      this.fetching = false;
    });
  }

  @action async uploadFile(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    if (this.currentFolder != null)
      formData.append('parentId', this.currentFolder.id);
    let result = await fetch(`${this.settings.api}/file`,
      {
        method: 'PUT',
        body: formData
      });
    let json = await result.json();
    runInAction(() => this.items.push(Item.fromJson(json)));
  }

  @observable folderCreating: boolean = false;
  @action async createFolder() {
    this.folderCreating = true;
    let result = await fetch(`${this.settings.api}/file/create-folder`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: this.folderName,
          parentid: this.currentFolder?.id
        })
      });
    let json = await result.json();
    runInAction(() => {
      this.items.push(Item.fromJson(json));
      this.setFolderName('');
      this.folderCreating = false;
    });
  }

  @action async getDownloadUrl(item: Item) {
    if (item.type != 'file')
      throw new Error('Only file type supported');
    let result = await fetch(`${this.settings.api}/file/download/${item.id}`);
    let json = await result.json();
    return json.url;
  }

  @action async setCurrentFolder(item: Item | null) {
    if (item != null && item.type != 'folder')
      throw new Error('Only folder type supported');
    this.currentFolder = item;
  }
}