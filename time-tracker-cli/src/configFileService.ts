import * as fs from 'fs';
import { FileNameBuilders } from './fileNameBuilders';
import { ConfigEntryModel } from './model/configEntryModel';

export class ConfigFileService {
    setConfig(token: ConfigEntryModel) {
      let json = this.stringifyConfigToJson(token);
      fs.writeFile(
        FileNameBuilders.getConfigFileName(),
        `${json}`,
        ( error ) => {
          if(error ) {
            console.error( error )
          }
        });
    }

    private stringifyConfigToJson(configEntry: ConfigEntryModel) {
      return JSON.stringify(configEntry, null, ' ');
    }
}
