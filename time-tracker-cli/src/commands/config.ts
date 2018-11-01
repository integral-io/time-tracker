import {Command, flags} from '@oclif/command'
const GClient = require('../client/gclient');
const global_config = require('../../config/oauth2-config.js');
const request = require('request');
const {google} = require('googleapis');

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
        return console.log(err);
      }

      console.log(body);
    });

    //var authCode = 0;
    // var get_data = querystring.stringify({
    //   authorizationCode: authCode,
    //   codeVerifier: codeVerifier
    // });
    // const url = this.oauth2Client.generateAuthUrl({
    //   access_type: 'offline',
    //   scope: global_config.scope
    // });
    // opn(url, function(err) {
    //   return err;
    // });
    // goauth2.getAuthCode(scope, function(err, auth_code) {
    //   goauth2.getTokensForAuthCode(auth_code, function(err, result) {
    //     console.log(result);
    //   });
    // });
    // googleapis.discover('plus', 'v1').execute(function(err, client) {
    //   if (err) return err;
    //   gauth({
    //     access_type: 'offline',
    //     scope: 'https://www.googleapis.com/auth/plus.me'
    //   }, {
    //     port: 8080,
    //     timeout: 5000
    //   }, function(err, authClient, tokens) {
    //     console.log("\r\n");
    //     console.log(tokens);
    //
    //     client.plus.people.get({
    //       userId: "me"
    //     }).withAuthClient(authClient)
    //       .execute(function(err, profile) {
    //         console.log("\r\n");
    //         console.log(profile, err);
    //         return err;
    //       });
    //   });
    // });
    // throw new Error("Method not implemented.");
  }
}
