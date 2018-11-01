import {Command, flags} from '@oclif/command'
const GClient = require('../client/gclient');
const global_config = require('../../config/oauth2-config.js');
const request = require('request');
const {google} = require('googleapis');
import { ConfigFileService } from '../configFileService';

export default class Config extends Command {
    
  static description = 'Configure the cli. Can set your name, email, projects and categories';

  static examples = [
    `$ itt config`,
  ]

  private google_client = new GClient(global_config.client_id, global_config.scope, global_config.redirect_uri_path);

  async run(): Promise<any> {
    const {args, flags} = this.parse(Config);
    let req_args = await this.google_client.authenticate();
    

    request({
      uri: 'http://localhost:5000/auth/google/exchange',
      qs: req_args
    }, (err, res, body) => {
      if (err) {
        console.log(req_args);
        return console.log(err);
      }

      let json = JSON.parse(body);
      let configService = new ConfigFileService();
      return configService.setLoginInfo(json);
    });
  }
}
