/*!
 * Copyright (c) Microsoft Corporation.
 * All rights reserved. See LICENSE in the project root for license information.
 */

import { buildMockModuleProps } from '@msdyn365-commerce/core-internal';
/// <reference types="jest" />

import * as React from 'react';
import * as renderer from 'react-test-renderer';

import ConfigVisibility from '../config-visibility';
import { IConfigVisibilityData } from '../config-visibility.data';
import { IConfigVisibilityConfig, IConfigVisibilityProps } from '../config-visibility.props.autogenerated';

const mockData: IConfigVisibilityData = {
    actionResponse: {
        text: 'Sample Response Data'
    }
};

const mockConfig: IConfigVisibilityConfig = {
    productTitle: 'title',
    subTitle: 'sub title'
};

const mockActions = {};

describe('ConfigVisibility', () => {
    let moduleProps: IConfigVisibilityProps<IConfigVisibilityData>;
    beforeAll(() => {
        moduleProps = buildMockModuleProps(mockData, mockActions, mockConfig) as IConfigVisibilityProps<IConfigVisibilityData>;
    });
    it('renders correctly', () => {
        const component: renderer.ReactTestRenderer = renderer.create(<ConfigVisibility {...moduleProps} />);
        const tree: renderer.ReactTestRendererJSON | renderer.ReactTestRendererJSON[] | null = component.toJSON();
        expect(tree).toMatchSnapshot();
    });
});
