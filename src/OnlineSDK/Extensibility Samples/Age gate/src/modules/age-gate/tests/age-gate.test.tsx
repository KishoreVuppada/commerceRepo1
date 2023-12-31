/*!
 * Copyright (c) Microsoft Corporation.
 * All rights reserved. See LICENSE in the project root for license information.
 */

import { buildMockModuleProps } from '@msdyn365-commerce/core-internal';
//  <reference types="jest" />

import * as React from 'react';
import * as renderer from 'react-test-renderer';

import AgeGate from '../age-gate';
import { IAgeGateConfig, IAgeGateProps } from '../age-gate.props.autogenerated';

const mockConfig: IAgeGateConfig = {
    heading: 'Age gate'
};

const mockActions = {};

describe('AgeGate', () => {
    let moduleProps: IAgeGateProps<IAgeGateConfig>;
    beforeAll(() => {
        moduleProps = buildMockModuleProps(mockActions, mockConfig) as IAgeGateProps<IAgeGateConfig>;
    });
    it('renders correctly', () => {
        const component: renderer.ReactTestRenderer = renderer.create(<AgeGate {...moduleProps} />);
        const tree: renderer.ReactTestRendererJSON | renderer.ReactTestRendererJSON[] | null = component.toJSON();
        expect(tree).toMatchSnapshot();
    });
});
