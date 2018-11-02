import {Command, flags} from '@oclif/command'
const GClient = require('../client/gclient');
const global_config = require('../../config/oauth2-config.js');
const request = require('request');
const {google} = require('googleapis');
import { ConfigFileService } from '../configFileService';
import {ConfigEntryModel} from "../model/configEntryModel";

export default class Config extends Command {
    
  static description = 'Configure the cli. Can set your name, email, projects and categories';

  static examples = [
    `$ itt config`,
  ]

  static flags = {
    help: flags.help(),
    server: flags.string({char: 's', description: 'Override the default server connection string.', default: 'https://localhost:5000'})
  }

  static args = [
    {name: 'server', description: 'The server to connect to for authentication.', required: false}
  ]

  async run(): Promise<any> {
    const {args, flags} = this.parse(Config);
    if (args.server == undefined) {
      args.server = 'https://localhost:5000';
    }

    let config_file_entry: ConfigEntryModel = {
      server: args.server,
      client_id: '',
      access_token: '',
      refresh_token: '',
      expires_in: 0
    };

    await request({
      uri: `${args.server}/auth/google/clientId`
    }, (err, res, body) => {
      if (err) {
        console.log(err);
        return false;
      }

      let json = JSON.parse(body);
      config_file_entry.client_id = json.client_id;
      let google_client = new GClient(config_file_entry.client_id, global_config.scope, global_config.redirect_uri_path);

      try {
        google_client.authenticate((req_args) => {
          request({
            uri: `${args.server}/auth/google/exchange`,
            qs: req_args
          }, (err, res, body) => {
            if (err) {
              console.log(req_args);
              console.log(err);
              return false;
            }

            let json = JSON.parse(body);
            config_file_entry.access_token = json.access_token;
            config_file_entry.refresh_token = json.refresh_token;
            config_file_entry.expires_in = json.expires_in;
            let configService = new ConfigFileService();
            configService.setConfig(config_file_entry);
            return true;
          });
        });
      } catch(e) {
        console.log('Couldn\'t authenticate with google.');
        return false;
      }
    });
  }
}
